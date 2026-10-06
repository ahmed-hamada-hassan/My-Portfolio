using Application.Features.About_Item.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.About_Item.Command;

public sealed record UpdateAboutItemCommand(
    Guid AboutItemId,
    UpdateAboutItemDTO UpdateAboutItemRequest)
    : ICommand<Result>;
