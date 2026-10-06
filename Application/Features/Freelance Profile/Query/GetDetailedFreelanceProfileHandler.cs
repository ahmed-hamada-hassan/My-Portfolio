using Application.Features.Freelance_Profile.DTOs;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;

namespace Application.Features.Freelance_Profile.Query;

internal sealed class GetDetailedFreelanceProfileHandler : IQueryHandler<GetDetailedFreelanceProfileQuery, Result<GetDetailsFreelanceProfileDTO>>
{
    private readonly IFreelanceProfileQueryRepository _freelanceProfileRepository;

    public GetDetailedFreelanceProfileHandler(
        IFreelanceProfileQueryRepository freelanceProfileRepository)
    {
        _freelanceProfileRepository = freelanceProfileRepository;
    }

    public async ValueTask<Result<GetDetailsFreelanceProfileDTO>> Handle(
        GetDetailedFreelanceProfileQuery query,
        CancellationToken cancellationToken)
    {
        var freelanceProfile =
            await _freelanceProfileRepository.GetDetailsByIdAsync(
                query.FreelanceProfileId,
                cancellationToken);

        if (freelanceProfile is null)
        {
            return Result<GetDetailsFreelanceProfileDTO>.Failure(
                FreelanceProfileErrors.FreelanceProfileNotFound);
        }

        return Result<GetDetailsFreelanceProfileDTO>
            .Success(freelanceProfile);
    }
}