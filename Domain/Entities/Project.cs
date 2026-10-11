using Domain.Errors;
using Domain.Shared;
using System.Text.RegularExpressions;

namespace Domain.Entities;

public class Project
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public string? LiveURL { get; private set; }
    public string? GitHubURL { get; private set; }

    //Navigational Properties
    private List<ProjectImage> _images = [];
    public IReadOnlyCollection<ProjectImage> Images => _images.AsReadOnly();

    protected Project() { }

    private Project(string title, string description, DateOnly startDate, DateOnly? endDate, string? liveURL, string? gitHubURL)
    {
        Id = Guid.NewGuid();
        Title = title;
        Slug = GenerateSlug(Title, Id);
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        LiveURL = liveURL;
        GitHubURL = gitHubURL;
    }

    public static Result<Project> Create(string title, string description, DateOnly startDate, DateOnly? endDate,
        string? liveURL, string? gitHubURL)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result<Project>.Failure(ProjectErrors.InvalidTitle);
        if(string.IsNullOrWhiteSpace(description))
            return Result<Project>.Failure(ProjectErrors.InvalidDescription);
        if (endDate.HasValue && startDate > endDate.Value)
            return Result<Project>.Failure(ProjectErrors.InvalidDates);

        var project = new Project(title, description, startDate, endDate, liveURL, gitHubURL);

        return Result<Project>.Success(project);
    }

    public Result Update(string? title, string? description, DateOnly? startDate, DateOnly? endDate, string? liveURL, string? gitHubURL)
    {
        var newStartDate = startDate ?? StartDate;
        var newEndDate = endDate ?? EndDate;

        if (newEndDate.HasValue && newStartDate > newEndDate.Value)
            return Result.Failure(ProjectErrors.InvalidDates);

        StartDate = newStartDate;
        EndDate = newEndDate;

        if (!string.IsNullOrWhiteSpace(title))
            Title = title;

        if (!string.IsNullOrWhiteSpace(description))
            Description = description;

        if (!string.IsNullOrWhiteSpace(liveURL))
            LiveURL = liveURL;

        if(!string.IsNullOrWhiteSpace(gitHubURL))
            GitHubURL = gitHubURL;

        return Result.Success();
    }

    private static string GenerateSlug(string title, Guid id)
    {
        var slug = title.Trim().ToLowerInvariant();

        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9-]", "");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        var suffix = id.ToString("N")[..8];

        return $"{slug}-{suffix}";
    }

    public Result MarkAsOngoing()
    {
        EndDate = null;
        return Result.Success();
    }

    public Result<ProjectImage> AddProjectImage(string url, string publicId, string altText, byte displayOrder, bool isCover)
    {
        var projectImageResult = ProjectImage.Create(Id, url, publicId, altText, displayOrder, isCover);

        if (projectImageResult.IsFailure)
            return Result<ProjectImage>.Failure(projectImageResult.Error!);

        var projectImage = projectImageResult.Value!;

        _images.Add(projectImage);

        return Result<ProjectImage>.Success(projectImage);
    }

    public Result RemoveProjectImage(ProjectImage projectImage)
    {
        if (!_images.Remove(projectImage))
            return Result.Failure(ProjectImageErrors.ImageNotFound);

        return Result.Success();
    }
}
