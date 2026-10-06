using Application.Features.Contact_Links.DTOs;
using Application.Interfaces.Repositories;
using Dapper;

namespace Infrastructure.Data.Dapper.Repository;

internal sealed class ContactLinkQueryRepository : IContactLinkQueryRepository
{
    private readonly DapperContext _context;

    public ContactLinkQueryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GetContactLinkDTO>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = """
            SELECT
            	Name,
            	Contact,
            FROM
            	SocialLinks
            WHERE
            	IsVisible = 1
            ORDER BY DisplayOrder ASC;
            """;

        using var connection = _context.CreateConnection();
        var command = new CommandDefinition(query, cancellationToken: cancellationToken);
        var socialLinks = await connection.QueryAsync<GetContactLinkDTO>(command);

        return socialLinks.ToList();
    }

    public async Task<IReadOnlyList<GetDetailsContactLinkDTO>> GetDetailsAsync(CancellationToken cancellationToken = default)
    {
        var query = """
            SELECT
                Id,
                Name,
                Contact,
                DisplayOrder,
                IsVisible
            FROM
                SocialLinks
            ORDER BY DisplayOrder ASC;
            """;

        using var connection = _context.CreateConnection();
        var command = new CommandDefinition(query, cancellationToken: cancellationToken);
        var socialLinks = await connection.QueryAsync<GetDetailsContactLinkDTO>(command);

        return socialLinks.ToList();
    }

    public async Task<GetDetailsContactLinkDTO?> GetDetailsByIdAsync(Guid socialLinkId, CancellationToken cancellationToken = default)
    {
        var query = """
            SELECT
                Id,
                Name,
                Contact,
                DisplayOrder,
                IsVisible
            FROM
                SocialLinks
            WHERE
                Id = @SocialLinkId;
            """;

        using var connection = _context.CreateConnection();
        var command = new CommandDefinition(query, new { SocialLinkId = socialLinkId }, cancellationToken: cancellationToken);
        var socialLink = await connection.QueryFirstOrDefaultAsync<GetDetailsContactLinkDTO>(command);

        return socialLink;
    }
}
