using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Site_Settings.Command;

internal sealed class UpdateProfileImageHandler
    : ICommandHandler<UpdateProfileImageCommand, Result>
{
    private readonly ISiteSettingsCommandRepository _siteSettingsCommandRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProfileImageHandler> _logger;

    public UpdateProfileImageHandler(
        ISiteSettingsCommandRepository siteSettingsCommandRepository,
        IImageStorageService imageStorageService,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProfileImageHandler> logger)
    {
        _siteSettingsCommandRepository = siteSettingsCommandRepository;
        _imageStorageService = imageStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(
        UpdateProfileImageCommand command,
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

        var oldImageUrl = siteSettings.ProfileImageURL;
        var oldImagePublicId = siteSettings.ProfileImagePublicId;

        var uploadResult = await _imageStorageService.UploadImageAsync(command.ProfileImage, cancellationToken);

        if (uploadResult.IsFailure)
        {
            _logger.LogError(
                "Failed to upload profile image. ErrorCode: {ErrorCode}",
                uploadResult.Error!.Code);

            return Result.Failure(uploadResult.Error!);
        }

        var newImageUrl = uploadResult.Value.URL;
        var newImagePublicId = uploadResult.Value.PublicId;

        var updateResult = siteSettings.UpdateProfileImage(
            newImageUrl,
            newImagePublicId);

        if (updateResult.IsFailure)
        {
            await CleanupNewImageAsync(
                newImageUrl,
                newImagePublicId);

            return Result.Failure(updateResult.Error!);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to save profile image changes to the database.");

            await CleanupNewImageAsync(
                newImageUrl,
                newImagePublicId);

            throw;
        }

        if (!string.IsNullOrWhiteSpace(oldImagePublicId))
        {
            var deleteOldImageResult =
                await _imageStorageService.DeleteImageAsync(
                    oldImageUrl!,
                    oldImagePublicId);

            if (deleteOldImageResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to delete the old profile image. ErrorCode: {ErrorCode}",
                    deleteOldImageResult.Error!.Code);
            }
        }

        return Result.Success();
    }

    private async Task CleanupNewImageAsync(
        string imageUrl,
        string publicId)
    {
        var deleteResult = await _imageStorageService.DeleteImageAsync(
            imageUrl,
            publicId);

        if (deleteResult.IsFailure)
        {
            _logger.LogError(
                "Failed to clean up the newly uploaded profile image. " +
                "ErrorCode: {ErrorCode}",
                deleteResult.Error!.Code);
        }
    }
}