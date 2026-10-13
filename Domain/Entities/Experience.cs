using Domain.Enums;
using Domain.Errors;
using Domain.Shared;

namespace Domain.Entities;

public class Experience
{
    public Guid Id { get; private set; }
    public string CompanyName { get; private set; } = null!;
    public string Position { get; private set; } = null!;
    public ExperienceType Type { get; private set; }
    public string Description { get; private set; } = null!;
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    protected Experience() { }

    private Experience(string companyName, string position, ExperienceType type, string description, DateOnly startDate, DateOnly? endDate)
    {
        Id = Guid.NewGuid();
        CompanyName = companyName;
        Position = position;
        Type = type;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
    }

    public static Result<Experience> Create(string companyName, string position, ExperienceType type, string description, DateOnly startDate,
        DateOnly? endDate)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            return Result<Experience>.Failure(ExperienceErrors.InvalidCompany);

        if (string.IsNullOrWhiteSpace(position))
            return Result<Experience>.Failure(ExperienceErrors.InvalidPosition);

        if (!Enum.IsDefined(typeof(ExperienceType), type))
            return Result<Experience>.Failure(ExperienceErrors.InvalidType);

        if (string.IsNullOrWhiteSpace(description))
            return Result<Experience>.Failure(ExperienceErrors.InvalidDescription);

        if (endDate.HasValue && startDate > endDate.Value)
            return Result<Experience>.Failure(ExperienceErrors.InvalidDates);

        var experience = new Experience(companyName, position, type, description, startDate, endDate);

        return Result<Experience>.Success(experience);
    }

    public Result Update(string? companyName, string? position, ExperienceType? type, string? description, DateOnly? startDate, DateOnly? endDate)
    {
        var newStartDate = startDate ?? StartDate;
        var newEndDate = endDate ?? EndDate;

        // Validate the proposed state before changing any property.  A partial
        // update may change only one date, so the existing counterpart must be
        // included when evaluating the date-range invariant.
        if (newEndDate.HasValue && newStartDate > newEndDate.Value)
            return Result.Failure(ExperienceErrors.InvalidDates);

        if (!string.IsNullOrWhiteSpace(companyName))
            CompanyName = companyName;

        if (!string.IsNullOrWhiteSpace(position))
            Position = position;

        if (type.HasValue && Enum.IsDefined(typeof(ExperienceType), type.Value))
            Type = type.Value;

        if (!string.IsNullOrWhiteSpace(description))
            Description = description;

        if (startDate.HasValue)
            StartDate = startDate.Value;

        if (endDate.HasValue)
            EndDate = endDate.Value;

        return Result.Success();
    }

    public Result MarkAsOngoing()
    {
        EndDate = null;
        return Result.Success();
    }
}
