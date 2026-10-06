using Application.Features.Freelance_Profile.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Freelance_Profile.Command;

public sealed record CreateFreelanceProfileCommand(CreateFreelanceProfileDTO createFreelanceProfileRequest) : ICommand<Result<Guid>>;