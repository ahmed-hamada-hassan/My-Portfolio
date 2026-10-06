using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Project.Command;

internal sealed class MarkProjectAsOngoingHandler : ICommandHandler<MarkProjectAsOngoingCommand, Result>
{
    private readonly IProjectCommandRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkProjectAsOngoingHandler> _logger;

    public MarkProjectAsOngoingHandler(
        IProjectCommandRepository projectRepository,
        IUnitOfWork unitOfWork,
        ILogger<MarkProjectAsOngoingHandler> logger)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(MarkProjectAsOngoingCommand command, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);

        if (project is null)
        {
            _logger.LogWarning(
                "Attempted to mark a non-existent Project as ongoing. ProjectId: {ProjectId}",
                command.ProjectId);

            return Result.Failure(ProjectErrors.NotFound);
        }

        var result = project.MarkAsOngoing();

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Mark Project as ongoing failed due to domain validation. ProjectId: {ProjectId}, ErrorCode: {ErrorCode}",
                command.ProjectId,
                result.Error!.Code);

            return Result.Failure(result.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Project marked as ongoing successfully. ProjectId: {ProjectId}",
            command.ProjectId);

        return Result.Success();
    }
}
