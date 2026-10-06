using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Experience.Command;

internal sealed class DeleteExperienceHandler : ICommandHandler<DeleteExperienceCommand, Result>
{
    private readonly IExperienceCommandRepository _experienceCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteExperienceHandler> _logger;

    public DeleteExperienceHandler(
        IExperienceCommandRepository experienceCommandRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteExperienceHandler> logger)
    {
        _experienceCommandRepository = experienceCommandRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(DeleteExperienceCommand command, CancellationToken cancellationToken)
    {
        var experience = await _experienceCommandRepository.GetByIdAsync(command.experienceId, cancellationToken);

        if (experience is null)
        {
            _logger.LogWarning(
                "Attempted to delete a non-existent Experience. ExperienceId: {ExperienceId}",
                command.experienceId);

            return Result.Failure(Domain.Errors.ExperienceErrors.NotFound);
        }

        _experienceCommandRepository.Remove(experience);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Experience deleted successfully. ExperienceId: {ExperienceId}",
            command.experienceId);

        return Result.Success();
    }
}
