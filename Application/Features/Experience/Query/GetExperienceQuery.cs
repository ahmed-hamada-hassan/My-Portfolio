using Application.Features.Experience.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Experience.Query;

public sealed record GetExperienceQuery(Guid ExperienceId) : IQuery<Result<GetExperienceDetailsDTO>>;