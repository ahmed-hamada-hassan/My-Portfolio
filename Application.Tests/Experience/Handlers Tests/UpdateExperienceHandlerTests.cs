using Application.Features.Experience.Command;
using Application.Features.Experience.DTOs;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Experience.Handlers_Tests;

public class UpdateExperienceHandlerTests
{
    private readonly IExperienceCommandRepository _experienceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateExperienceHandler> _logger;
    private readonly UpdateExperienceHandler _handler;

    public UpdateExperienceHandlerTests()
    {
        _experienceRepository = A.Fake<IExperienceCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<UpdateExperienceHandler>>();

        _handler = new UpdateExperienceHandler(
            _experienceRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var experienceId = Guid.NewGuid();
        var experience = CreateExperience();
        var request = new UpdateExperienceDTO(
            CompanyName: "Updated Corp",
            Position: "Senior Engineer",
            Type: ExperienceType.Contract,
            Description: "Updated description.",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)),
            EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        );
        var command = new UpdateExperienceCommand(experienceId, request);

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .Returns(experience);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        experience.CompanyName.Should().Be("Updated Corp");
        experience.Position.Should().Be("Senior Engineer");
        experience.Type.Should().Be(ExperienceType.Contract);
        experience.Description.Should().Be("Updated description.");
        experience.StartDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)));
        experience.EndDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)));
    }

    [Fact]
    public async Task Handle_WhenExperienceNotFound_ReturnsNotFoundFailureResult()
    {
        // Arrange
        var experienceId = Guid.NewGuid();
        var request = new UpdateExperienceDTO(
            CompanyName: "New Corp",
            Position: null,
            Type: null,
            Description: null,
            StartDate: null,
            EndDate: null
        );
        var command = new UpdateExperienceCommand(experienceId, request);

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .Returns((Domain.Entities.Experience?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ExperienceErrors.NotFound);

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDomainUpdateFailsDueToInvalidDates_ReturnsFailureResult()
    {
        // Arrange
        var experienceId = Guid.NewGuid();
        var experience = CreateExperience();
        var request = new UpdateExperienceDTO(
            CompanyName: null,
            Position: null,
            Type: null,
            Description: null,
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2))
        );
        var command = new UpdateExperienceCommand(experienceId, request);

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .Returns(experience);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ExperienceErrors.InvalidDates);

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var experienceId = Guid.NewGuid();
        var experience = CreateExperience();
        var request = new UpdateExperienceDTO(
            CompanyName: "New Corp",
            Position: null,
            Type: null,
            Description: null,
            StartDate: null,
            EndDate: null
        );
        var command = new UpdateExperienceCommand(experienceId, request);

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .Returns(experience);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    private static Domain.Entities.Experience CreateExperience()
    {
        var result = Domain.Entities.Experience.Create(
            "Original Corp",
            "Junior Dev",
            ExperienceType.FullTime,
            "Original description.",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        );
        return result.Value!;
    }
}
