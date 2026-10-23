using Application.Features.Site_Settings.Command;
using Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Site_Settings.Validators_Tests;

public class UpdateSiteSettingsValidatorTests
{
    private readonly UpdateSiteSettingsValidator _validator;

    public UpdateSiteSettingsValidatorTests()
    {
        _validator = new UpdateSiteSettingsValidator();
    }

    [Fact]
    public void UpdateSiteSettingsValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var command = new UpdateSiteSettingsCommand(
            SiteTitle: "My Portfolio",
            FullName: "John Doe",
            JobTitle: "Software Engineer",
            AvailableForWork: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateSiteSettingsValidator_WithAllNullFields_SuccessfullyValidates()
    {
        // Arrange
        var command = new UpdateSiteSettingsCommand(null, null, null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateSiteSettingsValidator_WithEmptySiteTitle_FailsValidation(string siteTitle)
    {
        // Arrange
        var command = new UpdateSiteSettingsCommand(siteTitle, null, null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.SiteTitle);
    }

    [Fact]
    public void UpdateSiteSettingsValidator_WithSiteTitleExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longTitle = new string('A', MaxLength.SiteSettingsSiteTitle + 1);
        var command = new UpdateSiteSettingsCommand(longTitle, null, null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.SiteTitle);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateSiteSettingsValidator_WithEmptyFullName_FailsValidation(string fullName)
    {
        // Arrange
        var command = new UpdateSiteSettingsCommand(null, fullName, null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void UpdateSiteSettingsValidator_WithFullNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longName = new string('A', MaxLength.SiteSettingsFullName + 1);
        var command = new UpdateSiteSettingsCommand(null, longName, null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateSiteSettingsValidator_WithEmptyJobTitle_FailsValidation(string jobTitle)
    {
        // Arrange
        var command = new UpdateSiteSettingsCommand(null, null, jobTitle, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.JobTitle);
    }

    [Fact]
    public void UpdateSiteSettingsValidator_WithJobTitleExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longJob = new string('A', MaxLength.SiteSettingsJobTitle + 1);
        var command = new UpdateSiteSettingsCommand(null, null, longJob, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.JobTitle);
    }
}
