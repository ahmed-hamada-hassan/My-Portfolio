using Domain.Shared;
using Mediator;

namespace Application.Features.Project_Images.Command;

public record DeleteProjectImageCommand(Guid ProjectId, Guid ImageId) : ICommand<Result>;