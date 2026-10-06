using Application.Features.About_Item.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.About_Item.Query;

public sealed record GetDetailedAboutItemsQuery
    : IQuery<Result<IReadOnlyList<GetDetailsAboutItemDTO>>>;
