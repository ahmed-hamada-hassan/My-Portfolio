using Application.Features.Contact_Links.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Contact_Links.Query;

public sealed record GetDetailedContactLinksQuery : IQuery<Result<IReadOnlyList<GetDetailsContactLinkDTO>>>;
