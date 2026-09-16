using Domain.Shared;

namespace Domain.Errors;

public class ProjectErrors
{
    public static readonly Error InvalidTitle = new Error("InvalidTitle", "Title cannot be empty.");
    public static readonly Error InvalidSlug = new Error("InvalidSlug", "Slug cannot be empty.");
    public static readonly Error InvalidDescription = new Error("InvalidDescription", "Description cannot be empty.");
    public static readonly Error InvalidDates = new Error("InvalidDates", "Start date cannot be after end date.");
    public static readonly Error DuplicateTitle = new Error("DublicatedTitle", "Project with this title already exists.");
}
