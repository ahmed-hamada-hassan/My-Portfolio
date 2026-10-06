using Application.Features.Skill.DTO;
using Application.Interfaces.Repositories;
using Dapper;
using Domain.Enums;

namespace Infrastructure.Data.Dapper.Repository;

internal sealed class SkillQueryRepository : ISkillQueryRepository
{
    private readonly DapperContext _context;

    public SkillQueryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SkillResponseDTO>> GetAllAsync(SkillType? Type = null, CancellationToken cancellationToken = default)
    {
        var query = Type.HasValue?
          """
          SELECT 
              Id, Name, Type, Icon
          FROM 
              Skills
          WHERE 
            Type = @Type
          ORDER BY Name;
          """
        : 
          """
          SELECT 
              Id, Name, Type, Icon
          FROM 
              Skills
          ORDER BY Name;
          """;

        using var connection = _context.CreateConnection();
        var parameters = Type.HasValue ? new { Type } : null;
        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken);
        var skills = await connection.QueryAsync<SkillResponseDTO>(command);

        return skills.ToList();
    }
}
