using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Experience.Command;

internal sealed class MarkExperienceAsOngoingHandler : ICommandHandler<MarkExperienceAsOngoingCommand, Result>
{
    private readonly IExperienceCommandRepository _experienceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkExperienceAsOngoingHandler> _logger;

    public MarkExperienceAsOngoingHandler(
        IExperienceCommandRepository experienceRepository,
        IUnitOfWork unitOfWork,
        ILogger<MarkExperienceAsOngoingHandler> logger)
    {
        _experienceRepository = experienceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(MarkExperienceAsOngoingCommand command, CancellationToken cancellationToken)
    {
        var experience = await _experienceRepository.GetByIdAsync(command.ExperienceId, cancellationToken);

        if (experience is null)
        {
            _logger.LogWarning(
                "Attempted to mark a non-existent Experience as ongoing. ExperienceId: {ExperienceId}",
                command.ExperienceId);

            return Result.Failure(ExperienceErrors.NotFound);
        }

        var result = experience.MarkAsOngoing();

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Mark Experience as ongoing failed due to domain validation. ExperienceId: {ExperienceId}, ErrorCode: {ErrorCode}",
                command.ExperienceId,
                result.Error!.Code);

            return Result.Failure(result.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Experience marked as ongoing successfully. ExperienceId: {ExperienceId}",
            command.ExperienceId);

        return Result.Success();
    }
}
