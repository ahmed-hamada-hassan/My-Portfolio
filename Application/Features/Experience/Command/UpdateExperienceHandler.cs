using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Experience.Command;

internal sealed class UpdateExperienceHandler : ICommandHandler<UpdateExperienceCommand, Result>
{
    private readonly IExperienceCommandRepository _experienceCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateExperienceHandler> _logger;

    public UpdateExperienceHandler(
        IExperienceCommandRepository experienceCommandRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateExperienceHandler> logger)
    {
        _experienceCommandRepository = experienceCommandRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(UpdateExperienceCommand command, CancellationToken cancellationToken)
    {
        var experience = await _experienceCommandRepository.GetByIdAsync(command.experienceId, cancellationToken);

        if (experience is null)
        {
            _logger.LogWarning(
                "Attempted to update a non-existent Experience. ExperienceId: {ExperienceId}",
                command.experienceId);

            return Result.Failure(ExperienceErrors.NotFound);
        }

        var request = command.updateExperienceRequest;

        var updateResult = experience.Update(request.CompanyName, request.Position, request.Type, request.Description,
            request.StartDate, request.EndDate);

        if (updateResult.IsFailure)
        {
            _logger.LogWarning(
                "Experience update failed due to domain validation. ExperienceId: {ExperienceId}, ErrorCode: {ErrorCode}",
                command.experienceId,
                updateResult.Error!.Code);

            return Result.Failure(updateResult.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Experience updated successfully. ExperienceId: {ExperienceId}",
            command.experienceId);

        return Result.Success();
    }
}
