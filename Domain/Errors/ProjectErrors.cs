using Domain.Shared;

namespace Domain.Errors;

public class ProjectErrors
{
    public static readonly Error InvalidTitle = new Error("Project.InvalidTitle", "Title cannot be empty.");
    public static readonly Error InvalidDescription = new Error("Project.InvalidDescription", "Description cannot be empty.");
    public static readonly Error InvalidDates = new Error("Project.InvalidDates", "Start date cannot be after end date.");
    public static readonly Error DuplicateTitle = new Error("Project.DuplicateTitle", "Project with this title already exists.");
    public static readonly Error NotFound = new Error("Project.NotFound", "Project not found.");
}
