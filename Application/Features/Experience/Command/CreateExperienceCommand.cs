using Application.Features.Experience.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Experience.Command;

public sealed record CreateExperienceCommand(CreateExperienceDTO createExperienceRequest) : ICommand<Result<Guid>>;