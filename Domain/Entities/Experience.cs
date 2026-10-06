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
        if (companyName is not null &&
            string.IsNullOrWhiteSpace(companyName))
            return Result.Failure(
                ExperienceErrors.InvalidCompany);

        if (position is not null &&
            string.IsNullOrWhiteSpace(position))
            return Result.Failure(
                ExperienceErrors.InvalidPosition);

        if (description is not null &&
            string.IsNullOrWhiteSpace(description))
            return Result.Failure(
                ExperienceErrors.InvalidDescription);

        if (type.HasValue &&
            !Enum.IsDefined(typeof(ExperienceType), type.Value))
            return Result.Failure(
                ExperienceErrors.InvalidType);

        var newStartDate = startDate ?? StartDate;
        var newEndDate = endDate ?? EndDate;

        if (newEndDate.HasValue &&
            newStartDate > newEndDate.Value)
            return Result.Failure(
                ExperienceErrors.InvalidDates);

        if (companyName is not null)
            CompanyName = companyName;

        if (position is not null)
            Position = position;

        if (type.HasValue)
            Type = type.Value;

        if (description is not null)
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
