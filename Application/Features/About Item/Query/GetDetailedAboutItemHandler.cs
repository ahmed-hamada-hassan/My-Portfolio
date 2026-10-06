using Application.Features.About_Item.DTOs;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.About_Item.Query;

internal sealed class GetDetailedAboutItemHandler
    : IQueryHandler<
        GetDetailedAboutItemQuery,
        Result<GetDetailsAboutItemDTO>>
{
    private readonly IAboutItemQueryRepository _repository;
    private readonly ILogger<GetDetailedAboutItemHandler> _logger;

    public GetDetailedAboutItemHandler(
        IAboutItemQueryRepository repository,
        ILogger<GetDetailedAboutItemHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async ValueTask<Result<GetDetailsAboutItemDTO>> Handle(
        GetDetailedAboutItemQuery query,
        CancellationToken cancellationToken)
    {
        var aboutItem = await _repository.GetDetailsByIdAsync(
            query.AboutItemId,
            cancellationToken);

        if (aboutItem is null)
        {
            _logger.LogWarning(
                "Attempted to get details of a non-existent AboutItem. AboutItemId: {AboutItemId}",
                query.AboutItemId);
            return Result<GetDetailsAboutItemDTO>.Failure(
                AboutItemErrors.AboutItemNotFound);
        }

        return Result<GetDetailsAboutItemDTO>
            .Success(aboutItem);
    }
}