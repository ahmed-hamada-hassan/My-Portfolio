using Application.Features.Freelance_Profile.DTOs;
using Application.Interfaces.Repositories;
using Domain.Shared;
using Mediator;

namespace Application.Features.Freelance_Profile.Query;

internal sealed class GetFreelanceProfilesHandler: IQueryHandler<GetFreelanceProfilesQuery, Result<IReadOnlyList<GetFreelanceProfileDTO>>>
{
    private readonly IFreelanceProfileQueryRepository _freelanceProfileRepository;

    public GetFreelanceProfilesHandler(
        IFreelanceProfileQueryRepository freelanceProfileRepository)
    {
        _freelanceProfileRepository = freelanceProfileRepository;
    }

    public async ValueTask<Result<IReadOnlyList<GetFreelanceProfileDTO>>> Handle(
        GetFreelanceProfilesQuery query,
        CancellationToken cancellationToken)
    {
        var freelanceProfiles =
            await _freelanceProfileRepository.GetAllAsync(
                cancellationToken);

        return Result<IReadOnlyList<GetFreelanceProfileDTO>>
            .Success(freelanceProfiles);
    }
}