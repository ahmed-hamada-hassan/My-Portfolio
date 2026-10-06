using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Project.Command;

internal sealed class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, Result>
{
    private readonly IProjectCommandRepository _projectCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<DeleteProjectHandler> _logger;

    public DeleteProjectHandler(
        IProjectCommandRepository projectCommandRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        ILogger<DeleteProjectHandler> logger)
    {
        _projectCommandRepository = projectCommandRepository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectCommandRepository.GetByIdWithImagesAsync(request.ProjectId, cancellationToken);

        if (project is null)
        {
            _logger.LogWarning(
                "Attempted to delete a non-existent Project. ProjectId: {ProjectId}",
                request.ProjectId);

            return Result.Failure(ProjectErrors.NotFound);
        }

        var deleteImagesResult = await DeleteProjectImages(project, cancellationToken);

        if (deleteImagesResult.IsFailure)
            return Result.Failure(deleteImagesResult.Error!);

        _projectCommandRepository.Remove(project);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Project deleted successfully. ProjectId: {ProjectId}",
            request.ProjectId);

        return Result.Success();
    }

    private async Task<Result<bool>> DeleteProjectImages(
        Domain.Entities.Project project,
        CancellationToken cancellationToken)
    {
        foreach (var image in project.Images)
        {
            var result = await _imageStorageService.DeleteImageAsync(image.URL, image.PublicId);

            if (result.IsFailure || !result.Value)
                return Result<bool>.Failure(result.Error!);
        }

        return Result<bool>.Success(true);
    }
}
