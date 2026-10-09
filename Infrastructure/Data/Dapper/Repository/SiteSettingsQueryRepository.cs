using Application.Features.Site_Settings.DTOs;
using Application.Interfaces.Repositories;
using Dapper;

namespace Infrastructure.Data.Dapper.Repository;

internal sealed class SiteSettingsQueryRepository : ISiteSettingsQueryRepository
{
    private readonly DapperContext _context;

    public SiteSettingsQueryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<SiteSettingsResponse?> GetSiteSettingsAsync(CancellationToken cancellationToken)
    {
        const string query = @"
                    SELECT
	                    SiteTitle,
	                    FullName,
	                    JobTitle,
	                    ProfileImageURL,
	                    CVURL,
	                    AvailableForWork
                    FROM
	                    SiteSettings";

        using var connection = _context.CreateConnection();

        var command = new CommandDefinition(
        query,
        cancellationToken: cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<SiteSettingsResponse>(
            command);
    }
}
