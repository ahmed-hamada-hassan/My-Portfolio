using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Certificate.Command;

internal sealed class ClearCredentialURLHandler
    : ICommandHandler<ClearCredentialURLCommand, Result>
{
    private readonly ICertificateCommandRepository _certificateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClearCredentialURLHandler> _logger;

    public ClearCredentialURLHandler(
        ICertificateCommandRepository certificateRepository,
        IUnitOfWork unitOfWork,
        ILogger<ClearCredentialURLHandler> logger)
    {
        _certificateRepository = certificateRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(
        ClearCredentialURLCommand command,
        CancellationToken cancellationToken)
    {
        var certificate = await _certificateRepository.GetByIdAsync(
            command.CertificateId,
            cancellationToken);

        if (certificate is null)
        {
            _logger.LogWarning(
                "Attempted to clear CredentialURL of a non-existent Certificate. CertificateId: {CertificateId}",
                command.CertificateId);

            return Result.Failure(
                CertificateErrors.NotFound);
        }

        var result = certificate.ClearCredentialURL();

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Failed to clear CredentialURL due to domain validation. CertificateId: {CertificateId}, ErrorCode: {ErrorCode}",
                command.CertificateId,
                result.Error!.Code);

            return Result.Failure(result.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Certificate CredentialURL cleared successfully. CertificateId: {CertificateId}",
            command.CertificateId);

        return Result.Success();
    }
}
