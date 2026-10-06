using Application.Features.About_Item.DTOs;
using Application.Interfaces.Repositories;
using Domain.Shared;
using Mediator;

namespace Application.Features.About_Item.Query;

internal sealed class GetAboutItemsHandler
    : IQueryHandler<
        GetAboutItemsQuery,
        Result<IReadOnlyList<GetAboutItemDTO>>>
{
    private readonly IAboutItemQueryRepository _repository;

    public GetAboutItemsHandler(
        IAboutItemQueryRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<Result<IReadOnlyList<GetAboutItemDTO>>> Handle(
        GetAboutItemsQuery query,
        CancellationToken cancellationToken)
    {
        var aboutItems = await _repository.GetAllAsync(
            cancellationToken);

        return Result<IReadOnlyList<GetAboutItemDTO>>
            .Success(aboutItems);
    }
}