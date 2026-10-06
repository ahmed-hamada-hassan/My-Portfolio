using Application.Features.About_Item.DTOs;
using Application.Interfaces.Repositories;
using Dapper;

namespace Infrastructure.Data.Dapper.Repository;

internal sealed class AboutItemQueryRepository
    : IAboutItemQueryRepository
{
    private readonly DapperContext _context;

    public AboutItemQueryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GetAboutItemDTO>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var query = """
            SELECT
                Title,
                Description
            FROM AboutItems
            WHERE IsVisible = 1
            ORDER BY DisplayOrder ASC;
            """;

        using var connection = _context.CreateConnection();

        var command = new CommandDefinition(
            query,
            cancellationToken: cancellationToken);

        var aboutItems =
            await connection.QueryAsync<GetAboutItemDTO>(command);

        return aboutItems.ToList();
    }

    public async Task<IReadOnlyList<GetDetailsAboutItemDTO>> GetDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        var query = """
            SELECT
                Id,
                Title,
                Description,
                DisplayOrder,
                IsVisible
            FROM AboutItems
            ORDER BY DisplayOrder ASC;
            """;

        using var connection = _context.CreateConnection();

        var command = new CommandDefinition(
            query,
            cancellationToken: cancellationToken);

        var aboutItems =
            await connection.QueryAsync<GetDetailsAboutItemDTO>(command);

        return aboutItems.ToList();
    }

    public async Task<GetDetailsAboutItemDTO?> GetDetailsByIdAsync(
        Guid aboutItemId,
        CancellationToken cancellationToken = default)
    {
        var query = """
            SELECT
                Id,
                Title,
                Description,
                DisplayOrder,
                IsVisible
            FROM AboutItems
            WHERE Id = @AboutItemId;
            """;

        using var connection = _context.CreateConnection();

        var command = new CommandDefinition(
            query,
            new { AboutItemId = aboutItemId },
            cancellationToken: cancellationToken);

        var aboutItem =
            await connection.QueryFirstOrDefaultAsync<
                GetDetailsAboutItemDTO>(command);

        return aboutItem;
    }
}