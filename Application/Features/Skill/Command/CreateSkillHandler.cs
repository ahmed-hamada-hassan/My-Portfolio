using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Skill.Command;

internal sealed class CreateSkillHandler : ICommandHandler<CreateSkillCommand, Result<Guid>>
{
    private readonly ISkillCommandRepository _skillCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateSkillHandler> _logger;

    public CreateSkillHandler(
        ISkillCommandRepository skillCommandRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateSkillHandler> logger)
    {
        _skillCommandRepository = skillCommandRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<Guid>> Handle(CreateSkillCommand command, CancellationToken cancellationToken)
    {
        var request = command.CreateSkillRequest;

        var nameExists = await _skillCommandRepository.NameExistsAsync(request.Name, null, cancellationToken);
        if (nameExists)
        {
            _logger.LogWarning(
                "Attempted to create Skill with an existing name.");

            return Result<Guid>.Failure(SkillErrors.SkillAlreadyExist);
        }

        var skillResult = Domain.Entities.Skill.Create(request.Name, request.Type, request.Icon);
        if (skillResult.IsFailure)
        {
            _logger.LogWarning(
                "Skill creation failed due to domain validation. ErrorCode: {ErrorCode}",
                skillResult.Error!.Code);

            return Result<Guid>.Failure(skillResult.Error!);
        }

        var skill = skillResult.Value!;

        await _skillCommandRepository.AddAsync(skill, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Skill created successfully. SkillId: {SkillId}",
            skill.Id);

        return Result<Guid>.Success(skill.Id);
    }
}
