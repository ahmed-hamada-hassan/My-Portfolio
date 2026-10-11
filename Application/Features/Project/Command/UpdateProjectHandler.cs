using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Project.Command;

internal sealed class UpdateProjectHandler : ICommandHandler<UpdateProjectCommand, Result>
{
    private readonly IProjectCommandRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProjectHandler> _logger;

    public UpdateProjectHandler(
        IProjectCommandRepository projectRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProjectHandler> logger)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        var request = command.UpdateProjectRequest;
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);

        if (project is null)
        {
            _logger.LogWarning(
                "Attempted to update a non-existent Project. ProjectId: {ProjectId}",
                command.ProjectId);

            return Result.Failure(Domain.Errors.ProjectErrors.NotFound);
        }

        if (await _projectRepository.TitleExistsAsync(string.IsNullOrWhiteSpace(request.Title) ? project.Title : request.Title,
            command.ProjectId, cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to update Project with an existing title. ProjectId: {ProjectId}",
                command.ProjectId);

            return Result.Failure(Domain.Errors.ProjectErrors.DuplicateTitle);
        }

        var result = project.Update(request.Title, request.Description, request.StartDate, request.EndDate, request.LiveURL, request.GitHubURL);

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Project update failed due to domain validation. ProjectId: {ProjectId}, ErrorCode: {ErrorCode}",
                command.ProjectId,
                result.Error!.Code);

            return Result.Failure(result.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Project updated successfully. ProjectId: {ProjectId}",
            command.ProjectId);

        return Result.Success();
    }
}
