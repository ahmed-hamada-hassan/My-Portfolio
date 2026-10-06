using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Skill.Command;

internal sealed class UpdateSkillHandler : ICommandHandler<UpdateSkillCommand, Result>
{
    private readonly ISkillCommandRepository _skillCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSkillHandler> _logger;

    public UpdateSkillHandler(
        ISkillCommandRepository skillCommandRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateSkillHandler> logger)
    {
        _skillCommandRepository = skillCommandRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(UpdateSkillCommand command, CancellationToken cancellationToken)
    {
        var skill = await _skillCommandRepository.GetByIdAsync(command.SkillId, cancellationToken);

        if (skill is null)
        {
            _logger.LogWarning(
                "Attempted to update a non-existent Skill. SkillId: {SkillId}",
                command.SkillId);

            return Result.Failure(SkillErrors.NotFound);
        }

        var request = command.UpdateSkillRequest;

        if (request.Name is not null)
        {
            var nameExists = await _skillCommandRepository.NameExistsAsync(request.Name, command.SkillId, cancellationToken);
            if (nameExists)
            {
                _logger.LogWarning(
                    "Attempted to update Skill with an existing name. SkillId: {SkillId}",
                    command.SkillId);

                return Result.Failure(SkillErrors.SkillAlreadyExist);
            }
        }

        var updateResult = skill.Update(request.Name, request.Type, request.Icon);
        if (updateResult.IsFailure)
        {
            _logger.LogWarning(
                "Skill update failed due to domain validation. SkillId: {SkillId}, ErrorCode: {ErrorCode}",
                command.SkillId,
                updateResult.Error!.Code);

            return Result.Failure(updateResult.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Skill updated successfully. SkillId: {SkillId}",
            command.SkillId);

        return Result.Success();
    }
}
