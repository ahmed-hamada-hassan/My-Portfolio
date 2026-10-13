using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Certificate.Command;

internal sealed class UpdateCertificateImageHandler : ICommandHandler<UpdateCertificateImageCommand, Result>
{
    private readonly ICertificateCommandRepository _certificateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<UpdateCertificateImageHandler> _logger;

    public UpdateCertificateImageHandler(
        ICertificateCommandRepository certificateRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        ILogger<UpdateCertificateImageHandler> logger)
    {
        _certificateRepository = certificateRepository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(UpdateCertificateImageCommand command, CancellationToken cancellationToken)
    {
        var certificate = await _certificateRepository.GetByIdAsync(command.CertificateId, cancellationToken);

        if (certificate is null)
        {
            _logger.LogWarning(
                "Attempted to update image of a non-existent Certificate. CertificateId: {CertificateId}",
                command.CertificateId);

            return Result.Failure(CertificateErrors.NotFound);
        }

        var oldImagePublicId = certificate.ImagePublicId;
        var oldImageURL = certificate.ImageURL;

        var uploadResult = await _imageStorageService.UploadImageAsync(command.CertificateImage, cancellationToken);

        if (uploadResult.IsFailure)
            return Result.Failure(uploadResult.Error!);

        var newImageURL = uploadResult.Value!.URL;
        var newImagePublicId = uploadResult.Value.PublicId;

        var updateResult = certificate.UpdateImage(newImageURL, newImagePublicId);
        if (updateResult.IsFailure)
        {
            _logger.LogWarning(
                "Certificate image update failed due to domain validation. CertificateId: {CertificateId}, ErrorCode: {ErrorCode}",
                command.CertificateId,
                updateResult.Error!.Code);

            await _imageStorageService.DeleteImageAsync(newImageURL, newImagePublicId);
            return Result.Failure(updateResult.Error!);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await _imageStorageService.DeleteImageAsync(newImageURL, newImagePublicId);
            throw;
        }

        await _imageStorageService.DeleteImageAsync(
            oldImageURL,
            oldImagePublicId);

        _logger.LogInformation(
            "Certificate image updated successfully. CertificateId: {CertificateId}",
            command.CertificateId);

        return Result.Success();
    }
}