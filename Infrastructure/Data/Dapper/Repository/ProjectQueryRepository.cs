using Application.Features.Project.DTOs;
using Application.Features.Skill.DTO;
using Application.Interfaces.Repositories;
using Application.Shared;
using Dapper;
using Domain.Enums;

namespace Infrastructure.Data.Dapper.Repository;

internal sealed class ProjectQueryRepository : IProjectQueryRepository
{
    private readonly DapperContext _context;

    public ProjectQueryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProjectListItemResponse>> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();

        const string query = """
            WITH PagedProjects AS 
            (
                SELECT
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.Description,
                    p.StartDate,
                    p.EndDate,
                    p.LiveURL,
                    p.GitHubURL,
                    COUNT(*) OVER() AS TotalCount
                FROM Projects p
                ORDER BY p.StartDate DESC, p.Id ASC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            )
            SELECT
                pp.Id AS ProjectId,
                pp.Title,
                pp.Slug,
                pp.Description,
                pp.StartDate,
                pp.EndDate,
                pp.LiveURL,
                pp.GitHubURL,
                pp.TotalCount,
                pi.URL AS CoverImageURL,
                s.Id AS SkillId,
                s.Name AS SkillName,
                s.Type AS SkillType,
                s.Icon AS SkillIcon
            FROM PagedProjects pp
            LEFT JOIN ProjectImages pi
                ON pp.Id = pi.ProjectId
                AND pi.IsCover = 1
            LEFT JOIN ProjectSkills ps
                ON pp.Id = ps.ProjectId
            LEFT JOIN Skills s
                ON ps.SkillId = s.Id
            ORDER BY pp.StartDate DESC, pp.Id ASC;
            """;

        var parameters = new
        {
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<ProjectListItemRow>(command);

        var projectDictionary = new Dictionary<Guid, ProjectListItemResponse>();

        foreach (var row in rows)
        {
            if (!projectDictionary.TryGetValue(row.ProjectId, out var project))
            {
                project = new ProjectListItemResponse(
                    row.ProjectId,
                    row.Title,
                    row.Slug,
                    row.Description,
                    row.StartDate,
                    row.EndDate,
                    row.LiveURL,
                    row.GitHubURL,
                    row.CoverImageURL!,
                    []);

                projectDictionary.Add(row.ProjectId, project);
            }

            if (row.SkillId.HasValue)
            {
                project.Skills.Add(
                    new SkillResponseDTO(
                        row.SkillId.Value,
                        row.SkillName!,
                        row.SkillType!.Value,
                        row.SkillIcon!));
            }
        }

        var totalCount = rows.FirstOrDefault()?.TotalCount ?? 0;

        return new PagedResult<ProjectListItemResponse>(
            projectDictionary.Values.ToList(),
            page,
            pageSize,
            totalCount);
    }

    public async Task<ProjectDetailsResponse?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();

        var query = """
                        SELECT
            	p.Id AS ProjectId,
            	p.Title,
            	p.Slug,
            	p.Description,
            	p.StartDate,
            	p.EndDate,
            	p.LiveURL,
            	p.GitHubURL,

            	pi.Id AS ImageId,
            	pi.URL,
            	pi.AltText,
            	pi.DisplayOrder,
            	pi.IsCover,

            	s.Id AS SkillId,
            	s.Name AS SkillName,
            	s.Type AS SkillType,
            	s.Icon AS SkillIcon
            FROM 
            	Projects p
            LEFT JOIN
            	ProjectImages pi ON p.Id = pi.ProjectId
            LEFT JOIN
            	ProjectSkills ps ON p.Id = ps.ProjectId
            LEFT JOIN
            	Skills s ON ps.SkillId = s.Id
            WHERE
            	p.Id = @ProjectId
            """;

        var command = new CommandDefinition(query, new { ProjectId = projectId }, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<ProjectDetailsRow>(command);
        var projectDictionary = new Dictionary<Guid, ProjectDetailsResponse>();
        var imageIds = new HashSet<Guid>();
        var skillIds = new HashSet<Guid>();

        foreach (var row in rows)
        {
            if (!projectDictionary.TryGetValue(row.ProjectId, out var project))
            {
                project = new ProjectDetailsResponse(
                    row.ProjectId,
                    row.Title,
                    row.Slug,
                    row.Description,
                    row.StartDate,
                    row.EndDate,
                    row.LiveURL,
                    row.GitHubURL,
                    [],
                    []);
                projectDictionary.Add(row.ProjectId, project);
            }
            if (row.ImageId.HasValue && imageIds.Add(row.ImageId.Value))
            {
                project.ProjectImages.Add(
                    new ProjectImageResponse(
                        row.ImageId.Value,
                        row.URL!,
                        row.AltText!,
                        row.DisplayOrder!.Value,
                        row.IsCover!.Value));
            }
            if (row.SkillId.HasValue && skillIds.Add(row.SkillId.Value))
            {
                project.Skills.Add(
                    new SkillResponseDTO(
                        row.SkillId.Value,
                        row.SkillName!,
                        row.SkillType!.Value,
                        row.SkillIcon!));
            }
        }

        return projectDictionary.Values.FirstOrDefault();
    }

    public async Task<ProjectDetailsResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();

        var query = """
                        SELECT
            	p.Id AS ProjectId,
            	p.Title,
            	p.Slug,
            	p.Description,
            	p.StartDate,
            	p.EndDate,
            	p.LiveURL,
            	p.GitHubURL,

            	pi.Id AS ImageId,
            	pi.URL,
            	pi.AltText,
            	pi.DisplayOrder,
            	pi.IsCover,

            	s.Id AS SkillId,
            	s.Name AS SkillName,
            	s.Type AS SkillType,
            	s.Icon AS SkillIcon
            FROM 
            	Projects p
            LEFT JOIN
            	ProjectImages pi ON p.Id = pi.ProjectId
            LEFT JOIN
            	ProjectSkills ps ON p.Id = ps.ProjectId
            LEFT JOIN
            	Skills s ON ps.SkillId = s.Id
            WHERE
            	p.Slug = @Slug
            """;

        var command = new CommandDefinition(query, new { Slug = slug }, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<ProjectDetailsRow>(command);
        var projectDictionary = new Dictionary<Guid, ProjectDetailsResponse>();
        var imageIds = new HashSet<Guid>();
        var skillIds = new HashSet<Guid>();

        foreach (var row in rows)
        {
            if (!projectDictionary.TryGetValue(row.ProjectId, out var project))
            {
                project = new ProjectDetailsResponse(
                    row.ProjectId,
                    row.Title,
                    row.Slug,
                    row.Description,
                    row.StartDate,
                    row.EndDate,
                    row.LiveURL,
                    row.GitHubURL,
                    [],
                    []);
                projectDictionary.Add(row.ProjectId, project);
            }
            if (row.ImageId.HasValue)
            {
                project.ProjectImages.Add(
                    new ProjectImageResponse(
                        row.ImageId.Value,
                        row.URL!,
                        row.AltText!,
                        row.DisplayOrder!.Value,
                        row.IsCover!.Value));
            }
            if (row.SkillId.HasValue)
            {
                project.Skills.Add(
                    new SkillResponseDTO(
                        row.SkillId.Value,
                        row.SkillName!,
                        row.SkillType!.Value,
                        row.SkillIcon!));
            }
        }

        return projectDictionary.Values.FirstOrDefault();
    }

    private sealed class ProjectListItemRow
    {
        public Guid ProjectId { get; init; }
        public string Title { get; init; } = null!;
        public string Slug { get; init; } = null!;
        public string Description { get; init; } = null!;
        public DateOnly StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        public string? LiveURL { get; init; }
        public string? GitHubURL { get; init; }
        public int TotalCount { get; init; }
        public string? CoverImageURL { get; init; }

        public Guid? SkillId { get; init; }
        public string? SkillName { get; init; }
        public SkillType? SkillType { get; init; }
        public string? SkillIcon { get; init; }
    }
    private sealed class ProjectDetailsRow
    {
        public Guid ProjectId { get; init; }
        public string Title { get; init; } = null!;
        public string Slug { get; init; } = null!;
        public string Description { get; init; } = null!;
        public DateOnly StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        public string? LiveURL { get; init; }
        public string? GitHubURL { get; init; }

        public Guid? ImageId { get; init; }
        public string? URL { get; init; }
        public string? AltText { get; init; }
        public byte? DisplayOrder { get; init; }
        public bool? IsCover { get; init; }

        public Guid? SkillId { get; init; }
        public string? SkillName { get; init; }
        public SkillType? SkillType { get; init; }
        public string? SkillIcon { get; init; }
    }
}