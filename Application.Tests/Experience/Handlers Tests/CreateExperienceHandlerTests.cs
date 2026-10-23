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

public class CreateExperienceHandlerTests
{
    private readonly IExperienceCommandRepository _experienceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateExperienceHandler> _logger;
    private readonly CreateExperienceHandler _handler;

    public CreateExperienceHandlerTests()
    {
        _experienceRepository = A.Fake<IExperienceCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<CreateExperienceHandler>>();

        _handler = new CreateExperienceHandler(
            _experienceRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessfulIdResult()
    {
        // Arrange
        var request = new CreateExperienceDTO(
            CompanyName: "Google",
            Position: "Software Engineer",
            Type: ExperienceType.FullTime,
            Description: "Developed scalable backend systems.",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        );
        var command = new CreateExperienceCommand(request);

        A.CallTo(() => _experienceRepository.AddAsync(A<Domain.Entities.Experience>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        A.CallTo(() => _experienceRepository.AddAsync(A<Domain.Entities.Experience>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreateExperienceDTO(
            CompanyName: "",
            Position: "Software Engineer",
            Type: ExperienceType.FullTime,
            Description: "Developed backend systems.",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            EndDate: null
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ExperienceErrors.InvalidCompany);

        A.CallTo(() => _experienceRepository.AddAsync(A<Domain.Entities.Experience>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenStartDateIsAfterEndDate_ReturnsInvalidDatesFailureResult()
    {
        // Arrange
        var request = new CreateExperienceDTO(
            CompanyName: "Tech Corp",
            Position: "Developer",
            Type: ExperienceType.PartTime,
            Description: "Worked on client apps.",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2))
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ExperienceErrors.InvalidDates);

        A.CallTo(() => _experienceRepository.AddAsync(A<Domain.Entities.Experience>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var request = new CreateExperienceDTO(
            CompanyName: "Tech Corp",
            Position: "Developer",
            Type: ExperienceType.FullTime,
            Description: "Valid description.",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            EndDate: null
        );
        var command = new CreateExperienceCommand(request);

        A.CallTo(() => _experienceRepository.AddAsync(A<Domain.Entities.Experience>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _experienceRepository.AddAsync(A<Domain.Entities.Experience>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
