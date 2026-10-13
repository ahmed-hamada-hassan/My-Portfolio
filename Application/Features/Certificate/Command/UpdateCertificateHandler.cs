using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Certificate.Command;

internal sealed class UpdateCertificateHandler : ICommandHandler<UpdateCertificateCommand, Result>
{
    private readonly ICertificateCommandRepository _certificateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCertificateHandler> _logger;

    public UpdateCertificateHandler(
        ICertificateCommandRepository certificateRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCertificateHandler> logger)
    {
        _certificateRepository = certificateRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(UpdateCertificateCommand command, CancellationToken cancellationToken)
    {
        var request = command.UpdateCertificateRequest;

        var certificate = await _certificateRepository.GetByIdAsync(command.CertificateId, cancellationToken);

        if (certificate is null)
        {
            _logger.LogWarning(
                "Attempted to update a non-existent Certificate. CertificateId: {CertificateId}",
                command.CertificateId);

            return Result.Failure(CertificateErrors.NotFound);
        }

        if (request.CredentialURL is not null)
        {
            var credentialURLExists = await _certificateRepository
                .CredentialURLExistsAsync(request.CredentialURL, command.CertificateId, cancellationToken);

            if (credentialURLExists)
            {
                _logger.LogWarning(
                    "Attempted to update Certificate with an existing CredentialURL. CertificateId: {CertificateId}",
                    command.CertificateId);

                return Result.Failure(CertificateErrors.CredentialUrlAlreadyExists);
            }
        }

        var updateResult = certificate.Update(request.Name, request.Issuer, request.IssuerLogoURL, request.IssueDate, request.CredentialURL);

        if (updateResult.IsFailure)
        {
            _logger.LogWarning(
                "Certificate update failed due to domain validation. CertificateId: {CertificateId}, ErrorCode: {ErrorCode}",
                command.CertificateId,
                updateResult.Error!.Code);

            return Result.Failure(updateResult.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Certificate updated successfully. CertificateId: {CertificateId}",
            command.CertificateId);

        return Result.Success();
    }
}
