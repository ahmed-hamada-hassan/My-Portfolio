using Application.Features.Project.Command;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Project.Validators_Tests;

public class DeleteProjectValidatorTests
{
    private readonly DeleteProjectValidator _validator;

    public DeleteProjectValidatorTests()
    {
        _validator = new DeleteProjectValidator();
    }

    [Fact]
    public void DeleteProjectValidator_WithValidId_SuccessfullyValidates()
    {
        // Arrange
        var command = new DeleteProjectCommand(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DeleteProjectValidator_WithEmptyId_FailsValidation()
    {
        // Arrange
        var command = new DeleteProjectCommand(Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ProjectId)
            .WithErrorMessage("Project ID is required.");
    }
}
