using Application.Features.Freelance_Profile.DTOs;
using Application.Interfaces.Repositories;
using Dapper;

namespace Infrastructure.Data.Dapper.Repository;

internal sealed class FreelanceProfileQueryRepository : IFreelanceProfileQueryRepository
{
    private readonly DapperContext _context;

    public FreelanceProfileQueryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GetFreelanceProfileDTO>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = """
            SELECT
                PlatformName,
                ProfileURL,
                Icon
            FROM FreelanceProfiles
            WHERE IsVisible = 1
            ORDER BY DisplayOrder ASC;
            """;

        using var connection = _context.CreateConnection();

        var command = new CommandDefinition(
            query,
            cancellationToken: cancellationToken);

        var freelanceProfiles =
            await connection.QueryAsync<GetFreelanceProfileDTO>(command);

        return freelanceProfiles.ToList();
    }

    public async Task<IReadOnlyList<GetDetailsFreelanceProfileDTO>> GetDetailsAsync(CancellationToken cancellationToken = default)
    {
        var query = """
            SELECT
                Id,
                PlatformName,
                ProfileURL,
                Icon,
                DisplayOrder,
                IsVisible
            FROM FreelanceProfiles
            ORDER BY DisplayOrder ASC;
            """;

        using var connection = _context.CreateConnection();

        var command = new CommandDefinition(
            query,
            cancellationToken: cancellationToken);

        var freelanceProfiles =
            await connection.QueryAsync<GetDetailsFreelanceProfileDTO>(command);

        return freelanceProfiles.ToList();
    }

    public async Task<GetDetailsFreelanceProfileDTO?> GetDetailsByIdAsync(Guid freelanceProfileId, CancellationToken cancellationToken = default)
    {
        var query = """
            SELECT
                Id,
                PlatformName,
                ProfileURL,
                Icon,
                DisplayOrder,
                IsVisible
            FROM FreelanceProfiles
            WHERE Id = @FreelanceProfileId;
            """;

        using var connection = _context.CreateConnection();

        var command = new CommandDefinition(
            query,
            new { FreelanceProfileId = freelanceProfileId },
            cancellationToken: cancellationToken);

        var freelanceProfile =
            await connection.QueryFirstOrDefaultAsync<
                GetDetailsFreelanceProfileDTO>(command);

        return freelanceProfile;
    }
}
