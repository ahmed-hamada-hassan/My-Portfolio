using Application.Features.About_Item.DTOs;
using Application.Interfaces.DI;

namespace Application.Interfaces.Repositories;

public interface IAboutItemQueryRepository : IScopedMark
{
    Task<IReadOnlyList<GetAboutItemDTO>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GetDetailsAboutItemDTO>> GetDetailsAsync(
        CancellationToken cancellationToken = default);

    Task<GetDetailsAboutItemDTO?> GetDetailsByIdAsync(
        Guid aboutItemId,
        CancellationToken cancellationToken = default);
}
