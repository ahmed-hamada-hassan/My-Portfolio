using Application.Interfaces.DI;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IAboutItemCommandRepository : IScopedMark
{
    Task<AboutItem?> GetByIdAsync(
        Guid aboutItemId,
        CancellationToken cancellationToken = default);

    Task<bool> TitleExistsAsync(
        string title,
        Guid? aboutItemId = null,
        CancellationToken cancellationToken = default);

    Task<bool> DisplayOrderExistsAsync(
        byte displayOrder,
        Guid? aboutItemId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AboutItem aboutItem,
        CancellationToken cancellationToken = default);

    void Remove(AboutItem aboutItem);
}
