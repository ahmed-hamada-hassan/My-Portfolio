using Domain.Entities;
using Domain.Enums;
using Domain.Errors;

namespace Domain.Tests;

public class ExperienceTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessfulExperienceResult()
    {
        // Arrange
        var companyName = "Company Name Test";
        var position = "Position Test";
        var experienceType = ExperienceType.Freelance;
        var description = "Description Test";
        var startDate = new DateOnly(2020, 1, 1);
        var endDate = new DateOnly(2021, 1, 1);

        // Act
        var result = Experience.Create(companyName, position, experienceType, description, startDate, endDate);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Value);

        var experience = result.Value;

        Assert.Equal(companyName, experience.CompanyName);
        Assert.Equal(position, experience.Position);
        Assert.Equal(experienceType, experience.Type);
        Assert.Equal(description, experience.Description);
        Assert.Equal(startDate, experience.StartDate);
        Assert.Equal(endDate, experience.EndDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidCompanyName_ReturnsInvalidCompanyNameError(string? invalidCompanyName)
    {
        // Arrange
        var position = "Position Test";
        var experienceType = ExperienceType.Freelance;
        var description = "Description Test";
        var startDate = new DateOnly(2020, 1, 1);
        var endDate = new DateOnly(2021, 1, 1);
        
        // Act
        var result = Experience.Create(invalidCompanyName!, position, experienceType, description, startDate, endDate);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ExperienceErrors.InvalidCompany, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidPosition_ReturnsInvalidPositionError(string? invalidPosition)
    {
        // Arrange
        var companyName = "Company Name Test";
        var experienceType = ExperienceType.Freelance;
        var description = "Description Test";
        var startDate = new DateOnly(2020, 1, 1);
        var endDate = new DateOnly(2021, 1, 1);

        // Act
        var result = Experience.Create(companyName, invalidPosition!, experienceType, description, startDate, endDate);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ExperienceErrors.InvalidPosition, result.Error);
    }

    [Fact]
    public void Create_WithInvalidExperienceType_ReturnsInvalidExperienceTypeError()
    {
        // Arrange
        var companyName = "Company Name Test";
        var position = "Position Test";
        var invalidExperienceType = (ExperienceType)999; // Invalid enum value
        var description = "Description Test";
        var startDate = new DateOnly(2020, 1, 1);
        var endDate = new DateOnly(2021, 1, 1);
        
        // Act
        var result = Experience.Create(companyName, position, invalidExperienceType, description, startDate, endDate);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ExperienceErrors.InvalidType, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidDescription_ReturnsInvalidDescriptionError(string? invalidDescription)
    {
        // Arrange
        var companyName = "Company Name Test";
        var position = "Position Test";
        var experienceType = ExperienceType.Freelance;
        var startDate = new DateOnly(2020, 1, 1);
        var endDate = new DateOnly(2021, 1, 1);

        // Act
        var result = Experience.Create(companyName, position, experienceType, invalidDescription!, startDate, endDate);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ExperienceErrors.InvalidDescription, result.Error);
    }

    [Fact]
    public void Create_WithStartDateAfterEndDate_ReturnsInvalidDateError()
    {
        // Arrange
        var companyName = "Company Name Test";
        var position = "Position Test";
        var experienceType = ExperienceType.Freelance;
        var description = "Description Test";
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = new DateOnly(2020, 1, 1); // Start date is after end date
        
        // Act
        var result = Experience.Create(companyName, position, experienceType, description, startDate, endDate);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ExperienceErrors.InvalidDates, result.Error);
    }

    [Fact]
    public void Update_WithValidData_UpdatesExperienceSuccessfully()
    {
        // Arrange
        var experience = CreateValidExperience();

        var newCompanyName = "Updated Company Name";
        var newPosition = "Updated Position";
        var newExperienceType = ExperienceType.FullTime;
        var newDescription = "Updated Description";
        var newStartDate = new DateOnly(2021, 1, 1);
        var newEndDate = new DateOnly(2022, 1, 1);

        // Act
        var result = experience.Update(newCompanyName, newPosition, newExperienceType, newDescription, newStartDate, newEndDate);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newCompanyName, experience.CompanyName);
        Assert.Equal(newPosition, experience.Position);
        Assert.Equal(newExperienceType, experience.Type);
        Assert.Equal(newDescription, experience.Description);
        Assert.Equal(newStartDate, experience.StartDate);
        Assert.Equal(newEndDate, experience.EndDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WithInvalidCompanyName_KeepsOriginalCompanyName(string? newInvalidCompanyName)
    {
        // Arrange
        var experience = CreateValidExperience();

        var originalCompanyName = experience.CompanyName;

        var newPosition = "Updated Position";
        var newExperienceType = ExperienceType.FullTime;
        var newDescription = "Updated Description";
        var newStartDate = new DateOnly(2021, 1, 1);
        var newEndDate = new DateOnly(2022, 1, 1);
        
        // Act
        var result = experience.Update(newInvalidCompanyName!, newPosition, newExperienceType, newDescription, newStartDate, newEndDate);
        
        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(originalCompanyName, experience.CompanyName);
        Assert.Equal(newPosition, experience.Position);
        Assert.Equal(newExperienceType, experience.Type);
        Assert.Equal(newDescription, experience.Description);
        Assert.Equal(newStartDate, experience.StartDate);
        Assert.Equal(newEndDate, experience.EndDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WithInvalidPosition_KeepsOriginalPosition(string? newInvalidPosition)
    {
        // Arrange
        var experience = CreateValidExperience();

        var originalPosition = experience.Position;

        var newCompanyName = "Updated Company Name";
        var newExperienceType = ExperienceType.FullTime;
        var newDescription = "Updated Description";
        var newStartDate = new DateOnly(2021, 1, 1);
        var newEndDate = new DateOnly(2022, 1, 1);

        // Act
        var result = experience.Update(newCompanyName, newInvalidPosition!, newExperienceType, newDescription, newStartDate, newEndDate);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newCompanyName, experience.CompanyName);
        Assert.Equal(originalPosition, experience.Position);
        Assert.Equal(newExperienceType, experience.Type);
        Assert.Equal(newDescription, experience.Description);
        Assert.Equal(newStartDate, experience.StartDate);
        Assert.Equal(newEndDate, experience.EndDate);
    }

    [Fact]
    public void Update_WithInvalidExperienceType_KeepsOriginalExperienceType()
    {
        // Arrange
        var experience = CreateValidExperience();

        var originalExperienceType = experience.Type;

        var newCompanyName = "Updated Company Name";
        var newPosition = "Updated Position";
        var invalidExperienceType = (ExperienceType)999; // Invalid enum value
        var newDescription = "Updated Description";
        var newStartDate = new DateOnly(2021, 1, 1);
        var newEndDate = new DateOnly(2022, 1, 1);
        
        // Act
        var result = experience.Update(newCompanyName, newPosition, invalidExperienceType, newDescription, newStartDate, newEndDate);
        
        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newCompanyName, experience.CompanyName);
        Assert.Equal(newPosition, experience.Position);
        Assert.Equal(originalExperienceType, experience.Type);
        Assert.Equal(newDescription, experience.Description);
        Assert.Equal(newStartDate, experience.StartDate);
        Assert.Equal(newEndDate, experience.EndDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WithInvalidDescription_KeepsOriginalDescription(string? newInvalidDescription)
    {
        // Arrange
        var experience = CreateValidExperience();

        var originalDescription = experience.Description;

        var newCompanyName = "Updated Company Name";
        var newPosition = "Updated Position";
        var newExperienceType = ExperienceType.FullTime;
        var newStartDate = new DateOnly(2021, 1, 1);
        var newEndDate = new DateOnly(2022, 1, 1);

        // Act
        var result = experience.Update(newCompanyName, newPosition, newExperienceType, newInvalidDescription!, newStartDate, newEndDate);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newCompanyName, experience.CompanyName);
        Assert.Equal(newPosition, experience.Position);
        Assert.Equal(newExperienceType, experience.Type);
        Assert.Equal(originalDescription, experience.Description);
        Assert.Equal(newStartDate, experience.StartDate);
        Assert.Equal(newEndDate, experience.EndDate);
    }

    [Fact]
    public void Update_WithStartDateAfterEndDate_KeepsOriginalDates()
    {
        // Arrange
        var experience = CreateValidExperience();

        var originalStartDate = experience.StartDate;
        var originalEndDate = experience.EndDate;

        var newCompanyName = "Updated Company Name";
        var newPosition = "Updated Position";
        var newExperienceType = ExperienceType.FullTime;
        var newDescription = "Updated Description";
        var newStartDate = new DateOnly(2022, 1, 1); // Start date is after end date
        var newEndDate = new DateOnly(2021, 1, 1);

        // Act
        var result = experience.Update(newCompanyName, newPosition, newExperienceType, newDescription, newStartDate, newEndDate);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newCompanyName, experience.CompanyName);
        Assert.Equal(newPosition, experience.Position);
        Assert.Equal(newExperienceType, experience.Type);
        Assert.Equal(newDescription, experience.Description);
        Assert.Equal(originalStartDate, experience.StartDate);
        Assert.Equal(originalEndDate, experience.EndDate);
    }

    [Fact]
    public void MarkAsOngoing_WhenExperienceHasEndDate_SetsEndDateToNull()
    {
        // Arrange
        var experience = CreateValidExperience();

        // Act
        experience.MarkAsOngoing();

        // Assert
        Assert.Null(experience.EndDate);
    }

    private static Experience CreateValidExperience()
    {
        var companyName = "Company Name Test";
        var position = "Position Test";
        var experienceType = ExperienceType.Freelance;
        var description = "Description Test";
        var startDate = new DateOnly(2020, 1, 1);
        var endDate = new DateOnly(2021, 1, 1);
        var result = Experience.Create(companyName, position, experienceType, description, startDate, endDate);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return result.Value;
    }
}