using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Certificate.Command;

internal sealed class DeleteCertificateHandler : ICommandHandler<DeleteCertificateCommand, Result>
{
    private readonly ICertificateCommandRepository _certificateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<DeleteCertificateHandler> _logger;

    public DeleteCertificateHandler(
        ICertificateCommandRepository certificateRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        ILogger<DeleteCertificateHandler> logger)
    {
        _certificateRepository = certificateRepository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(DeleteCertificateCommand command, CancellationToken cancellationToken)
    {
        var certificate = await _certificateRepository.GetByIdAsync(command.CertificateId, cancellationToken);

        if (certificate is null)
        {
            _logger.LogWarning(
                "Attempted to delete a non-existent Certificate. CertificateId: {CertificateId}",
                command.CertificateId);

            return Result.Failure(Domain.Errors.CertificateErrors.NotFound);
        }

        var imageUrl = certificate.ImageURL;
        var imagePublicId = certificate.ImagePublicId;

        _certificateRepository.Remove(certificate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var deleteImageResult = await _imageStorageService.DeleteImageAsync(imageUrl, imagePublicId);
        if (deleteImageResult.IsFailure)
            return Result.Failure(deleteImageResult.Error!);

        _logger.LogInformation(
            "Certificate deleted successfully. CertificateId: {CertificateId}",
            command.CertificateId);

        return Result.Success();
    }
}
