using Application.Features.Skill.DTO;
using Application.Interfaces.Repositories;
using Domain.Shared;
using Mediator;

namespace Application.Features.Skill.Query;

internal sealed class GetAllSkillsHandler : IQueryHandler<GetAllSkillsQuery, Result<IReadOnlyList<SkillResponseDTO>>>
{
    private readonly ISkillQueryRepository _skillQueryRepository;

    public GetAllSkillsHandler(ISkillQueryRepository skillQueryRepository)
    {
        _skillQueryRepository = skillQueryRepository;
    }

    public async ValueTask<Result<IReadOnlyList<SkillResponseDTO>>> Handle(GetAllSkillsQuery query, CancellationToken cancellationToken)
    {
        var type = query.Type ?? null;
        var skillsResult = await _skillQueryRepository.GetAllAsync(type, cancellationToken);
        return Result<IReadOnlyList<SkillResponseDTO>>.Success(skillsResult);
    }
}
