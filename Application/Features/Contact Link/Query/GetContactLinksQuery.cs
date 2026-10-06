using Application.Features.Contact_Links.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Contact_Links.Query;

public sealed record GetContactLinksQuery : IQuery<Result<IReadOnlyList<GetContactLinkDTO>>>;