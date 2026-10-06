using Application.Features.Experience.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Experience.Command;

public sealed record UpdateExperienceCommand(Guid experienceId, UpdateExperienceDTO updateExperienceRequest) : ICommand<Result>;