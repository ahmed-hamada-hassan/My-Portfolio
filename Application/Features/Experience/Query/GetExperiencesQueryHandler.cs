using Application.Features.Experience.DTOs;
using Application.Interfaces.Repositories;
using Domain.Shared;
using Mediator;

namespace Application.Features.Experience.Query;

internal sealed class GetExperiencesQueryHandler : IQueryHandler<GetExperiencesQuery, Result<IReadOnlyList<GetExperienceListItemDTO>>>
{
    private readonly IExperienceQueryRepository _experienceRepository;

    public GetExperiencesQueryHandler(IExperienceQueryRepository experienceRepository)
    {
        _experienceRepository = experienceRepository;
    }

    public async ValueTask<Result<IReadOnlyList<GetExperienceListItemDTO>>> Handle(GetExperiencesQuery query, CancellationToken cancellationToken)
    {
        var experiences = await _experienceRepository.GetAllAsync(cancellationToken);

        return Result<IReadOnlyList<GetExperienceListItemDTO>>.Success(experiences);
    }
}
