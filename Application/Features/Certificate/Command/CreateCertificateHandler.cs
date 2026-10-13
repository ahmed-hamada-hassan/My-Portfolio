using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Certificate.Command;

internal sealed class CreateCertificateHandler : ICommandHandler<CreateCertificateCommand, Result<Guid>>
{
    private readonly ICertificateCommandRepository _certificateCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<CreateCertificateHandler> _logger;

    public CreateCertificateHandler(
        ICertificateCommandRepository certificateCommandRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        ILogger<CreateCertificateHandler> logger)
    {
        _certificateCommandRepository = certificateCommandRepository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async ValueTask<Result<Guid>> Handle(CreateCertificateCommand command, CancellationToken cancellationToken)
    {
        var request = command.createCertificateRequest;

        if (request.CredentialURL is not null)
        {
            var credentialURLExists = await _certificateCommandRepository
            .CredentialURLExistsAsync(request.CredentialURL, null, cancellationToken);

            if (credentialURLExists)
            {
                _logger.LogWarning(
                    "Attempted to create Certificate with an existing CredentialURL.");

                return Result<Guid>.Failure(CertificateErrors.CredentialUrlAlreadyExists);
            }
        }

        var certificateImageURLResult = await _imageStorageService.UploadImageAsync(request.Image, cancellationToken);
        if (certificateImageURLResult.IsFailure)
            return Result<Guid>.Failure(certificateImageURLResult.Error!);

        var certificateImageURL = certificateImageURLResult.Value.URL;
        var certificateImagePublicId = certificateImageURLResult.Value.PublicId;

        var certificateResult = Domain.Entities.Certificate
            .Create(request.Name, request.Issuer, request.IssuerLogoURL, request.IssueDate, request.CredentialURL,
            certificateImageURL, certificateImagePublicId);

        if (certificateResult.IsFailure)
        {
            _logger.LogWarning(
                "Certificate creation failed due to domain validation. ErrorCode: {ErrorCode}",
                certificateResult.Error!.Code);

            await _imageStorageService.DeleteImageAsync(certificateImageURL, certificateImagePublicId);
            return Result<Guid>.Failure(certificateResult.Error!);
        }

        var certificate = certificateResult.Value!;

        try
        {
            await _certificateCommandRepository.AddAsync(certificate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            await _imageStorageService.DeleteImageAsync(certificateImageURL, certificateImagePublicId);
            throw;
        }

        _logger.LogInformation(
            "Certificate created successfully. CertificateId: {CertificateId}",
            certificate.Id);

        return Result<Guid>.Success(certificate.Id);
    }
}
