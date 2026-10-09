using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.EF_Core.Repositories;

internal sealed class SiteSettingsCommandRepository : ISiteSettingsCommandRepository
{
    private readonly AppDbContext _dbContext;

    public SiteSettingsCommandRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SiteSettings?> GetSiteSettingsAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SiteSettings.FirstOrDefaultAsync(cancellationToken);
    }
}
