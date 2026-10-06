using Application.Features.Experience.DTOs;
using Application.Interfaces.Repositories;
using Dapper;

namespace Infrastructure.Data.Dapper.Repository;

internal sealed class ExperienceQueryRepository : IExperienceQueryRepository
{
    private readonly DapperContext _context;

    public ExperienceQueryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GetExperienceListItemDTO>> GetAllAsync(CancellationToken ct = default)
    {
        var query = """
            SELECT
            	Id AS ExperienceId,
            	CompanyName,
            	Position,
            	Type,
            	StartDate,
            	EndDate
            FROM
            	Experiences
            ORDER BY
            	StartDate DESC;
            """;

        using var connection = _context.CreateConnection();
        var command = new CommandDefinition(query, cancellationToken: ct);
        var Experiences = await connection.QueryAsync<GetExperienceListItemDTO>(command);

        return Experiences.ToList();
    }

    public async Task<GetExperienceDetailsDTO?> GetByIdAsync(Guid experienceId, CancellationToken ct = default)
    {
        var query = """
            SELECT
                Id AS ExperienceId,
                CompanyName,
                Position,
                Type,
                Description,
                StartDate,
                EndDate
            FROM
                Experiences
            WHERE
                Id = @ExperienceId;
            """;

        using var connection = _context.CreateConnection();
        var command = new CommandDefinition(query, new { ExperienceId = experienceId }, cancellationToken: ct);
        var experience = await connection.QuerySingleOrDefaultAsync<GetExperienceDetailsDTO>(command);

        return experience;
    }
}
