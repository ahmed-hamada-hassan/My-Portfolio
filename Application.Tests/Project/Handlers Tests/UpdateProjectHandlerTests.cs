using Application.Features.Project.Command;
using Application.Features.Project.DTOs;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Project.Handlers_Tests;

public class UpdateProjectHandlerTests
{
    private readonly IProjectCommandRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProjectHandler> _logger;
    private readonly UpdateProjectHandler _handler;

    public UpdateProjectHandlerTests()
    {
        _projectRepository = A.Fake<IProjectCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<UpdateProjectHandler>>();

        _handler = new UpdateProjectHandler(
            _projectRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_UpdatesProjectAndReturnsSuccessResult()
    {
        // Arrange
        var project = CreateProject();
        var request = new UpdateProjectDTO(
            Title: "Updated Title",
            Description: "Updated Description",
            StartDate: new DateOnly(2025, 2, 1),
            EndDate: new DateOnly(2025, 8, 1),
            LiveURL: "https://updated.example.com",
            GitHubURL: "https://github.com/example/updated-repo"
        );
        var command = new UpdateProjectCommand(project.Id, request);

        A.CallTo(() => _projectRepository.GetByIdAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _projectRepository.TitleExistsAsync(request.Title!, project.Id, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Title.Should().Be("Updated Title");
        project.Description.Should().Be("Updated Description");
        project.StartDate.Should().Be(new DateOnly(2025, 2, 1));
        project.EndDate.Should().Be(new DateOnly(2025, 8, 1));

        A.CallTo(() => _projectRepository.GetByIdAsync(project.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsNotFoundFailureResult()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var request = new UpdateProjectDTO("Title", null, null, null, null, null);
        var command = new UpdateProjectCommand(projectId, request);

        A.CallTo(() => _projectRepository.GetByIdAsync(projectId, A<CancellationToken>._))
            .Returns((Domain.Entities.Project?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.NotFound);

        A.CallTo(() => _projectRepository.GetByIdAsync(projectId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenTitleAlreadyExists_ReturnsDuplicateTitleFailureResult()
    {
        // Arrange
        var project = CreateProject();
        var request = new UpdateProjectDTO("Existing Title", null, null, null, null, null);
        var command = new UpdateProjectCommand(project.Id, request);

        A.CallTo(() => _projectRepository.GetByIdAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _projectRepository.TitleExistsAsync(request.Title!, project.Id, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.DuplicateTitle);

        A.CallTo(() => _projectRepository.TitleExistsAsync(request.Title!, project.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var project = CreateProject();
        // End date before start date
        var request = new UpdateProjectDTO(
            Title: null,
            Description: null,
            StartDate: new DateOnly(2025, 10, 1),
            EndDate: new DateOnly(2025, 1, 1),
            LiveURL: null,
            GitHubURL: null
        );
        var command = new UpdateProjectCommand(project.Id, request);

        A.CallTo(() => _projectRepository.GetByIdAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _projectRepository.TitleExistsAsync(project.Title, project.Id, A<CancellationToken>._))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.InvalidDates);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var project = CreateProject();
        var request = new UpdateProjectDTO("Updated Title", null, null, null, null, null);
        var command = new UpdateProjectCommand(project.Id, request);

        A.CallTo(() => _projectRepository.GetByIdAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _projectRepository.TitleExistsAsync(request.Title!, project.Id, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    private static Domain.Entities.Project CreateProject()
    {
        return Domain.Entities.Project.Create(
            "Original Title",
            "Original Description",
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 6, 1),
            "https://live.example.com",
            "https://github.com/example/repo"
        ).Value!;
    }
}
