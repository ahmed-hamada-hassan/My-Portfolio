using Domain.Shared;

namespace Domain.Errors;

public class ExperienceErrors
{
    public static readonly Error InvalidPosition = new Error("InvalidPosition", "Position cannot be empty.");
    public static readonly Error InvalidCompany = new Error("InvalidCompany", "Company cannot be empty.");
    public static readonly Error InvalidType = new Error("InvalidType", "Type must be a valid ExperienceType.");
    public static readonly Error InvalidDescription = new Error("InvalidDescription", "Description cannot be empty.");
    public static readonly Error InvalidDates = new Error("InvalidDates", "Start date cannot be after end date.");

}
