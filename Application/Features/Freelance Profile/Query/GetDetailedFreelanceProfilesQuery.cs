using Application.Features.Freelance_Profile.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Freelance_Profile.Query;

public sealed record GetDetailedFreelanceProfilesQuery : IQuery<Result<IReadOnlyList<GetDetailsFreelanceProfileDTO>>>;