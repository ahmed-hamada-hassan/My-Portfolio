using Application.Features.Site_Settings.DTOs;
using Application.Interfaces.DI;

namespace Application.Interfaces.Repositories;

public interface ISiteSettingsQueryRepository : IScopedMark
{
    Task<SiteSettingsResponse?> GetSiteSettingsAsync(CancellationToken cancellationToken);
}