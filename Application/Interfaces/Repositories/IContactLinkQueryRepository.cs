using Application.Features.Contact_Links.DTOs;
using Application.Interfaces.DI;

namespace Application.Interfaces.Repositories;

public interface IContactLinkQueryRepository : IScopedMark
{
    Task<IReadOnlyList<GetContactLinkDTO>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GetDetailsContactLinkDTO>> GetDetailsAsync(CancellationToken cancellationToken = default);
    Task<GetDetailsContactLinkDTO?> GetDetailsByIdAsync(Guid socialLinkId, CancellationToken cancellationToken = default);
}
