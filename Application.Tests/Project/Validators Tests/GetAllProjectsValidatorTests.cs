using Application.Features.Project.Queries;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Project.Validators_Tests;

public class GetAllProjectsValidatorTests
{
    private readonly GetAllProjectsValidator _validator;

    public GetAllProjectsValidatorTests()
    {
        _validator = new GetAllProjectsValidator();
    }

    [Fact]
    public void GetAllProjectsValidator_WithValidPagination_SuccessfullyValidates()
    {
        // Arrange
        var query = new GetAllProjectsQuery(1, 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetAllProjectsValidator_WithInvalidPage_FailsValidation(int page)
    {
        // Arrange
        var query = new GetAllProjectsQuery(page, 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage("Page must be greater than 0.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    [InlineData(100)]
    public void GetAllProjectsValidator_WithInvalidPageSize_FailsValidation(int pageSize)
    {
        // Arrange
        var query = new GetAllProjectsQuery(1, pageSize);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Page size must be between 1 and 50.");
    }
}
