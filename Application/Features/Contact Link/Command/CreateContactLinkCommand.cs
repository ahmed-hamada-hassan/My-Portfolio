using Application.Features.Contact_Links.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Contact_Links.Command;

public sealed record CreateContactLinkCommand(CreateContactLinkDTO CreateContactLinkRequest) : ICommand<Result<Guid>>;