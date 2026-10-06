using Domain.Shared;
using Mediator;

namespace Application.Features.About_Item.Command;

public sealed record DeleteAboutItemCommand(
    Guid AboutItemId)
    : ICommand<Result>;