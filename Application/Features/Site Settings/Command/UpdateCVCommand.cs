using Domain.Shared;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Site_Settings.Command;

public record UpdateCVCommand(IFormFile CV) : ICommand<Result>;