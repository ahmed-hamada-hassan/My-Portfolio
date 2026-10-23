using Application.Features.Experience.Command;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Experience.Handlers_Tests;

public class MarkExperienceAsOngoingHandlerTests
{
    private readonly IExperienceCommandRepository _experienceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkExperienceAsOngoingHandler> _logger;
    private readonly MarkExperienceAsOngoingHandler _handler;

    public MarkExperienceAsOngoingHandlerTests()
    {
        _experienceRepository = A.Fake<IExperienceCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<MarkExperienceAsOngoingHandler>>();

        _handler = new MarkExperienceAsOngoingHandler(
            _experienceRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidId_SetsEndDateToNullAndReturnsSuccess()
    {
        // Arrange
        var experienceId = Guid.NewGuid();
        var experience = CreateExperienceWithEndDate();

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .Returns(experience);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        var command = new MarkExperienceAsOngoingCommand(experienceId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        experience.EndDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenExperienceNotFound_ReturnsNotFoundFailureResult()
    {
        // Arrange
        var experienceId = Guid.NewGuid();

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .Returns((Domain.Entities.Experience?)null);

        var command = new MarkExperienceAsOngoingCommand(experienceId);

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
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var experienceId = Guid.NewGuid();
        var experience = CreateExperienceWithEndDate();

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .Returns(experience);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        var command = new MarkExperienceAsOngoingCommand(experienceId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _experienceRepository.GetByIdAsync(experienceId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    private static Domain.Entities.Experience CreateExperienceWithEndDate()
    {
        var result = Domain.Entities.Experience.Create(
            "Company",
            "Position",
            ExperienceType.FullTime,
            "Description",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        );
        return result.Value!;
    }
}
