using Application.Interfaces.DI;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ISiteSettingsCommandRepository : IScopedMark
{
    Task<SiteSettings?> GetSiteSettingsAsync(CancellationToken cancellationToken);
}
