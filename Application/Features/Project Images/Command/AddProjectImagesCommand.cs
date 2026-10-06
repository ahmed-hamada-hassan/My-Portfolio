using Application.Features.Project_Images.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Project_Images.Command;

public record AddProjectImagesCommand(Guid ProjectId, IReadOnlyList<AddProjectImageDTO> ProjectImages) : ICommand<Result>;