using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.EF_Core.Repositories;

internal sealed class ContactLinkCommandRepository : IContactLinkCommandRepository
{
    private readonly AppDbContext _dbContext;

    public ContactLinkCommandRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ContactLink socialLink, CancellationToken cancellationToken = default)
    {
        await _dbContext.SocialLinks.AddAsync(socialLink, cancellationToken);
    }

    public async Task<ContactLink?> GetByIdAsync(Guid SocialLinkId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SocialLinks.FirstOrDefaultAsync(sl => sl.Id == SocialLinkId, cancellationToken);
    }

    public void Remove(ContactLink socialLink)
    {
        _dbContext.SocialLinks.Remove(socialLink);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? socialLinkId = null, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SocialLinks
            .AnyAsync(sl => sl.Name == name && (socialLinkId == null || sl.Id != socialLinkId), cancellationToken);
    }

    public async Task<bool> URLExistsAsync(string url, Guid? socialLinkId = null, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SocialLinks
            .AnyAsync(sl => sl.Contact == url && (socialLinkId == null || sl.Id != socialLinkId), cancellationToken);
    }

    public async Task<bool> DisplayOrderExistsAsync(byte displayOrder, Guid? socialLinkId = null, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SocialLinks
            .AnyAsync(sl => sl.DisplayOrder == displayOrder && (socialLinkId == null || sl.Id != socialLinkId), cancellationToken);
    }
}
