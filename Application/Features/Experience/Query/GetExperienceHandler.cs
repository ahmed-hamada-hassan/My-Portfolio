using Application.Features.Experience.DTOs;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;

namespace Application.Features.Experience.Query;

internal sealed class GetExperienceHandler : IQueryHandler<GetExperienceQuery, Result<GetExperienceDetailsDTO>>
{
    private readonly IExperienceQueryRepository _experienceRepository;

    public GetExperienceHandler(IExperienceQueryRepository experienceRepository)
    {
        _experienceRepository = experienceRepository;
    }

    public async ValueTask<Result<GetExperienceDetailsDTO>> Handle(GetExperienceQuery query, CancellationToken cancellationToken)
    {
        var experience = await _experienceRepository.GetByIdAsync(query.ExperienceId, cancellationToken);
        if(experience is null)
            return Result<GetExperienceDetailsDTO>.Failure(ExperienceErrors.NotFound);

        return Result<GetExperienceDetailsDTO>.Success(experience);
    }
}
