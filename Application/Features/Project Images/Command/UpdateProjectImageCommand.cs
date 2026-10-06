using Application.Features.Project_Images.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Project_Images.Command;

public record UpdateProjectImageCommand(Guid ProjectId, Guid ImageId, UpdateProjectImageDTO UpdateProjectImageRequest) : ICommand<Result>;