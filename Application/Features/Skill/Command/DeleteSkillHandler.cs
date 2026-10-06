using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Skill.Command;

internal sealed class DeleteSkillHandler : ICommandHandler<DeleteSkillCommand, Result>
{
    private readonly ISkillCommandRepository _skillCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteSkillHandler> _logger;

    public DeleteSkillHandler(
        ISkillCommandRepository skillCommandRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteSkillHandler> logger)
    {
        _skillCommandRepository = skillCommandRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(DeleteSkillCommand command, CancellationToken cancellationToken)
    {
        var skill = await _skillCommandRepository.GetByIdAsync(command.SkillId, cancellationToken);

        if (skill is null)
        {
            _logger.LogWarning(
                "Attempted to delete a non-existent Skill. SkillId: {SkillId}",
                command.SkillId);

            return Result.Failure(SkillErrors.NotFound);
        }

        var isUsed = await _skillCommandRepository.IsUsedAsync(skill.Id, cancellationToken);
        if (isUsed)
        {
            _logger.LogWarning(
                "Attempted to delete a Skill that is currently in use. SkillId: {SkillId}",
                command.SkillId);

            return Result.Failure(SkillErrors.SkillIsUsed);
        }

        _skillCommandRepository.Remove(skill);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Skill deleted successfully. SkillId: {SkillId}",
            command.SkillId);

        return Result.Success();
    }
}
