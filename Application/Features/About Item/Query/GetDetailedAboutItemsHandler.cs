using Application.Features.About_Item.DTOs;
using Application.Interfaces.Repositories;
using Domain.Shared;
using Mediator;

namespace Application.Features.About_Item.Query;

internal sealed class GetDetailedAboutItemsHandler
    : IQueryHandler<
        GetDetailedAboutItemsQuery,
        Result<IReadOnlyList<GetDetailsAboutItemDTO>>>
{
    private readonly IAboutItemQueryRepository _repository;

    public GetDetailedAboutItemsHandler(
        IAboutItemQueryRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<Result<IReadOnlyList<GetDetailsAboutItemDTO>>> Handle(
        GetDetailedAboutItemsQuery query,
        CancellationToken cancellationToken)
    {
        var aboutItems = await _repository.GetDetailsAsync(
            cancellationToken);

        return Result<IReadOnlyList<GetDetailsAboutItemDTO>>
            .Success(aboutItems);
    }
}