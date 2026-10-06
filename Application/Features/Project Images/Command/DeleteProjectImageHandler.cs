using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Project_Images.Command;

internal sealed class DeleteProjectImageHandler : ICommandHandler<DeleteProjectImageCommand, Result>
{
    private readonly IProjectCommandRepository _projectCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<DeleteProjectImageHandler> _logger;

    public DeleteProjectImageHandler(
        IProjectCommandRepository projectCommandRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        ILogger<DeleteProjectImageHandler> logger)
    {
        _projectCommandRepository = projectCommandRepository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(
        DeleteProjectImageCommand command,
        CancellationToken cancellationToken)
    {
        var project = await _projectCommandRepository.GetByIdWithImagesAsync(
            command.ProjectId,
            cancellationToken);

        if (project is null)
        {
            _logger.LogWarning(
                "Attempted to delete image from a non-existent Project. ProjectId: {ProjectId}",
                command.ProjectId);

            return Result.Failure(ProjectErrors.NotFound);
        }

        var projectImage = project.Images
            .FirstOrDefault(pi => pi.Id == command.ImageId);

        if (projectImage is null)
        {
            _logger.LogWarning(
                "Attempted to delete a non-existent ProjectImage. ProjectId: {ProjectId}, ImageId: {ImageId}",
                command.ProjectId,
                command.ImageId);

            return Result.Failure(ProjectImageErrors.ImageNotFound);
        }

        var imageUrl = projectImage.URL;
        var publicId = projectImage.PublicId;

        var removeResult = project.RemoveProjectImage(projectImage);

        if (removeResult.IsFailure)
        {
            _logger.LogWarning(
                "ProjectImage removal failed due to domain validation. ProjectId: {ProjectId}, ImageId: {ImageId}, ErrorCode: {ErrorCode}",
                command.ProjectId,
                command.ImageId,
                removeResult.Error!.Code);

            return Result.Failure(removeResult.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var deleteResult = await _imageStorageService.DeleteImageAsync(imageUrl, publicId);

        if (deleteResult.IsFailure)
            return Result.Failure(deleteResult.Error!);

        _logger.LogInformation(
            "ProjectImage deleted successfully. ProjectId: {ProjectId}, ImageId: {ImageId}",
            command.ProjectId,
            command.ImageId);

        return Result.Success();
    }
}
