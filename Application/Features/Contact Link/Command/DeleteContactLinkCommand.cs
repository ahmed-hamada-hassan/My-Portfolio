using Domain.Shared;
using Mediator;

namespace Application.Features.Contact_Links.Command;

public sealed record DeleteContactLinkCommand(Guid contactLinkId) : ICommand<Result>;