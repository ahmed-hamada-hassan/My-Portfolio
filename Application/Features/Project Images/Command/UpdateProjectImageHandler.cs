using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Project_Images.Command;

internal sealed class UpdateProjectImageHandler : ICommandHandler<UpdateProjectImageCommand, Result>
{
    private readonly IProjectCommandRepository _projectCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<UpdateProjectImageHandler> _logger;

    public UpdateProjectImageHandler(
        IProjectCommandRepository projectCommandRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        ILogger<UpdateProjectImageHandler> logger)
    {
        _projectCommandRepository = projectCommandRepository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(
        UpdateProjectImageCommand command,
        CancellationToken cancellationToken)
    {
        var project = await _projectCommandRepository.GetByIdWithImagesAsync(
            command.ProjectId,
            cancellationToken);

        if (project is null)
        {
            _logger.LogWarning(
                "Attempted to update image of a non-existent Project. ProjectId: {ProjectId}",
                command.ProjectId);

            return Result.Failure(ProjectErrors.NotFound);
        }

        var projectImage = project.Images
            .FirstOrDefault(pi => pi.Id == command.ImageId);

        if (projectImage is null)
        {
            _logger.LogWarning(
                "Attempted to update a non-existent ProjectImage. ProjectId: {ProjectId}, ImageId: {ImageId}",
                command.ProjectId,
                command.ImageId);

            return Result.Failure(ProjectImageErrors.ImageNotFound);
        }

        var request = command.UpdateProjectImageRequest;

        string? oldUrl = null;
        string? oldPublicId = null;

        string? newUrl = null;
        string? newPublicId = null;

        if (request.Image is not null)
        {
            oldUrl = projectImage.URL;
            oldPublicId = projectImage.PublicId;

            var uploadResult = await _imageStorageService.UploadImageAsync(
                request.Image,
                cancellationToken);

            if (uploadResult.IsFailure)
                return Result.Failure(uploadResult.Error!);

            var uploadedImage = uploadResult.Value!;

            newUrl = uploadedImage.URL;
            newPublicId = uploadedImage.PublicId;

            var updateResult = projectImage.Update(
                newUrl,
                newPublicId,
                request.AltText,
                request.DisplayOrder,
                request.IsCover);

            if (updateResult.IsFailure)
            {
                _logger.LogWarning(
                    "ProjectImage update failed due to domain validation. ProjectId: {ProjectId}, ImageId: {ImageId}, ErrorCode: {ErrorCode}",
                    command.ProjectId,
                    command.ImageId,
                    updateResult.Error!.Code);

                await _imageStorageService.DeleteImageAsync(newUrl, newPublicId);

                return Result.Failure(updateResult.Error!);
            }
        }
        else
        {
            var updateResult = projectImage.Update(
                null,
                null,
                request.AltText,
                request.DisplayOrder,
                request.IsCover);

            if (updateResult.IsFailure)
            {
                _logger.LogWarning(
                    "ProjectImage update failed due to domain validation. ProjectId: {ProjectId}, ImageId: {ImageId}, ErrorCode: {ErrorCode}",
                    command.ProjectId,
                    command.ImageId,
                    updateResult.Error!.Code);

                return Result.Failure(updateResult.Error!);
            }
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (newUrl is not null && newPublicId is not null)
            {
                await _imageStorageService.DeleteImageAsync(
                    newUrl,
                    newPublicId);
            }

            throw;
        }

        if (oldUrl is not null && oldPublicId is not null)
        {
            await _imageStorageService.DeleteImageAsync(
                oldUrl,
                oldPublicId);
        }

        _logger.LogInformation(
            "ProjectImage updated successfully. ProjectId: {ProjectId}, ImageId: {ImageId}",
            command.ProjectId,
            command.ImageId);

        return Result.Success();
    }
}