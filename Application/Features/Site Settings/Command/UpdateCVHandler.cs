using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Site_Settings.Command;

internal sealed class UpdateCVHandler
    : ICommandHandler<UpdateCVCommand, Result>
{
    private readonly ISiteSettingsCommandRepository _siteSettingsCommandRepository;
    private readonly IDocumentStorageService _documentStorageService;
    private readonly ILogger<UpdateCVHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCVHandler(
        ISiteSettingsCommandRepository siteSettingsCommandRepository,
        IDocumentStorageService documentStorageService,
        ILogger<UpdateCVHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _siteSettingsCommandRepository = siteSettingsCommandRepository;
        _documentStorageService = documentStorageService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(
        UpdateCVCommand command,
        CancellationToken cancellationToken)
    {
        var siteSettings =
            await _siteSettingsCommandRepository.GetSiteSettingsAsync(
                cancellationToken);

        if (siteSettings is null)
        {
            _logger.LogWarning("Site settings were not found.");

            return Result.Failure(
                SiteSettingsErrors.SiteSettingsNotFound);
        }

        var oldCVUrl = siteSettings.CVURL;

        var uploadResult = await _documentStorageService.UploadAsync(
            command.CV,
            cancellationToken);

        if (uploadResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to upload CV. ErrorCode: {ErrorCode}",
                uploadResult.Error!.Code);

            return Result.Failure(uploadResult.Error!);
        }

        var newCVUrl = uploadResult.Value!;

        var updateResult = siteSettings.UpdateCv(newCVUrl);

        if (updateResult.IsFailure)
        {
            await CleanupNewCVAsync(newCVUrl);

            return Result.Failure(updateResult.Error!);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save CV changes to the database.");

            await CleanupNewCVAsync(newCVUrl);

            throw;
        }

        if (!string.IsNullOrWhiteSpace(oldCVUrl))
        {
            var deleteOldCVResult =
                await _documentStorageService.DeleteAsync(
                    oldCVUrl,
                    cancellationToken);

            if (deleteOldCVResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to delete old CV. ErrorCode: {ErrorCode}",
                    deleteOldCVResult.Error!.Code);
            }
        }

        return Result.Success();
    }

    private async Task CleanupNewCVAsync(string cvUrl)
    {
        try
        {
            var deleteResult = await _documentStorageService.DeleteAsync(
                cvUrl,
                CancellationToken.None);

            if (deleteResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to clean up newly uploaded CV. ErrorCode: {ErrorCode}",
                    deleteResult.Error!.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An exception occurred while cleaning up the newly uploaded CV.");
        }
    }
}