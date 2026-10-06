using Application.Interfaces.DI;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IContactLinkCommandRepository : IScopedMark
{
    Task<ContactLink?> GetByIdAsync(Guid SocialLinkId, CancellationToken cancellationToken = default);
    Task AddAsync(ContactLink socialLink, CancellationToken cancellationToken = default);
    void Remove(ContactLink socialLink);
    Task<bool> NameExistsAsync(string name, Guid? socialLinkId = null, CancellationToken cancellationToken = default);
    Task<bool> URLExistsAsync(string url, Guid? socialLinkId = null, CancellationToken cancellationToken = default);
    Task<bool> DisplayOrderExistsAsync(byte displayOrder, Guid? socialLinkId = null, CancellationToken cancellationToken = default);
}
