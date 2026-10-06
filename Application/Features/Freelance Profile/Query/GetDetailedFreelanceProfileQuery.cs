using Application.Features.Freelance_Profile.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Freelance_Profile.Query;

public sealed record GetDetailedFreelanceProfileQuery(Guid FreelanceProfileId) : IQuery<Result<GetDetailsFreelanceProfileDTO>>;