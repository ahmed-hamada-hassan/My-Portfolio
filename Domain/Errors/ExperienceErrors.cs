using Domain.Shared;

namespace Domain.Errors;

public class ExperienceErrors
{
    public static readonly Error InvalidPosition = new Error("Experience.InvalidPosition", "Position cannot be empty.");
    public static readonly Error InvalidCompany = new Error("Experience.InvalidCompany", "Company cannot be empty.");
    public static readonly Error InvalidType = new Error("Experience.InvalidType", "Type must be a valid ExperienceType.");
    public static readonly Error InvalidDescription = new Error("Experience.InvalidDescription", "Description cannot be empty.");
    public static readonly Error InvalidDates = new Error("Experience.InvalidDates", "Start date cannot be after end date.");
    public static readonly Error NotFound = new Error("Experience.NotFound", "Experience not found.");
}
