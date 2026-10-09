using Domain.Shared;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Site_Settings.Command;

public record UpdateProfileImageCommand(IFormFile ProfileImage) : ICommand<Result>;