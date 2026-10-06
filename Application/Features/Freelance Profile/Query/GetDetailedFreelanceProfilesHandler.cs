using Application.Features.Freelance_Profile.DTOs;
using Application.Interfaces.Repositories;
using Domain.Shared;
using Mediator;

namespace Application.Features.Freelance_Profile.Query;

internal sealed class GetDetailedFreelanceProfilesHandler : IQueryHandler<GetDetailedFreelanceProfilesQuery, 
    Result<IReadOnlyList<GetDetailsFreelanceProfileDTO>>>
{
    private readonly IFreelanceProfileQueryRepository _freelanceProfileRepository;

    public GetDetailedFreelanceProfilesHandler(
        IFreelanceProfileQueryRepository freelanceProfileRepository)
    {
        _freelanceProfileRepository = freelanceProfileRepository;
    }

    public async ValueTask<
        Result<IReadOnlyList<GetDetailsFreelanceProfileDTO>>> Handle(
        GetDetailedFreelanceProfilesQuery query,
        CancellationToken cancellationToken)
    {
        var freelanceProfiles =
            await _freelanceProfileRepository.GetDetailsAsync(
                cancellationToken);

        return Result<IReadOnlyList<GetDetailsFreelanceProfileDTO>>
            .Success(freelanceProfiles);
    }
}