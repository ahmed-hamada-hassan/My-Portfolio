using Application.Features.Project_Images.DTOs;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Project_Images.Command;

internal sealed class AddProjectImagesHandler : ICommandHandler<AddProjectImagesCommand, Result>
{
    private readonly IProjectCommandRepository _projectCommandRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddProjectImagesHandler> _logger;

    public AddProjectImagesHandler(
        IProjectCommandRepository projectCommandRepository,
        IImageStorageService imageStorageService,
        IUnitOfWork unitOfWork,
        ILogger<AddProjectImagesHandler> logger)
    {
        _projectCommandRepository = projectCommandRepository;
        _imageStorageService = imageStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(AddProjectImagesCommand command, CancellationToken cancellationToken)
    {
        var project = await _projectCommandRepository.GetByIdWithImagesAsync(command.ProjectId, cancellationToken);

        if (project is null)
        {
            _logger.LogWarning(
                "Attempted to add images to a non-existent Project. ProjectId: {ProjectId}",
                command.ProjectId);

            return Result.Failure(ProjectErrors.NotFound);
        }

        var currentProjectImagesCount = project.Images.Count;
        var newProjectImages = command.ProjectImages.Count;
        var totalImages = currentProjectImagesCount + newProjectImages;

        if (totalImages > 8)
        {
            _logger.LogWarning(
                "Attempted to exceed the maximum image limit for Project. ProjectId: {ProjectId}",
                command.ProjectId);

            return Result.Failure(ProjectImageErrors.MaxImagesExceeded);
        }

        var uploadedImages = new List<(string Url, string PublicId)>();

        var imageResult = await AddProjectImages(project, command.ProjectImages, uploadedImages, cancellationToken);

        if (imageResult.IsFailure)
            return Result.Failure(imageResult.Error!);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await CleanupUploadedImages(uploadedImages);

            throw;
        }

        _logger.LogInformation(
            "Project images added successfully. ProjectId: {ProjectId}, ImagesAdded: {ImagesAdded}",
            command.ProjectId,
            newProjectImages);

        return Result.Success();
    }

    private async Task<Result> AddProjectImages(Domain.Entities.Project project, IEnumerable<AddProjectImageDTO> images,
        List<(string Url, string PublicId)> uploadedImages,
        CancellationToken cancellationToken)
    {
        foreach (var image in images)
        {
            var uploadResult = await _imageStorageService.UploadImageAsync(
                image.Image,
                cancellationToken);

            if (uploadResult.IsFailure)
            {
                await CleanupUploadedImages(uploadedImages);

                return Result.Failure(uploadResult.Error!);
            }

            var uploadedImage = uploadResult.Value!;

            uploadedImages.Add((
                uploadedImage.URL,
                uploadedImage.PublicId));

            var addImageResult = project.AddProjectImage(
                uploadedImage.URL,
                uploadedImage.PublicId,
                image.AltText,
                image.DisplayOrder,
                image.IsCover);

            if (addImageResult.IsFailure)
            {
                await CleanupUploadedImages(uploadedImages);

                return Result.Failure(addImageResult.Error!);
            }
        }

        return Result.Success();
    }

    private async Task CleanupUploadedImages(IEnumerable<(string Url, string PublicId)> images)
    {
        foreach (var image in images)
        {
            await _imageStorageService.DeleteImageAsync(
                image.Url,
                image.PublicId);
        }
    }
}
