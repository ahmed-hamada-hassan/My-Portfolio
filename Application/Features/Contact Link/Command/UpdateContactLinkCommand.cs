using Application.Features.Contact_Links.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Contact_Links.Command;

public sealed record UpdateContactLinkCommand(Guid contactLinkId, UpdateContactLinkDTO updateContactLinkRequest) : ICommand<Result>;