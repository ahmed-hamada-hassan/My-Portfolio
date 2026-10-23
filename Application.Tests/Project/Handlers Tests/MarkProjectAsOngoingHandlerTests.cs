using Application.Features.Project.Command;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Project.Handlers_Tests;

public class MarkProjectAsOngoingHandlerTests
{
    private readonly IProjectCommandRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkProjectAsOngoingHandler> _logger;
    private readonly MarkProjectAsOngoingHandler _handler;

    public MarkProjectAsOngoingHandlerTests()
    {
        _projectRepository = A.Fake<IProjectCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<MarkProjectAsOngoingHandler>>();

        _handler = new MarkProjectAsOngoingHandler(
            _projectRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WhenProjectExists_MarksOngoingAndReturnsSuccessResult()
    {
        // Arrange
        var project = Domain.Entities.Project.Create(
            "Title",
            "Description",
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 6, 1), // Has EndDate
            null,
            null
        ).Value!;
        var command = new MarkProjectAsOngoingCommand(project.Id);

        A.CallTo(() => _projectRepository.GetByIdAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.EndDate.Should().BeNull();

        A.CallTo(() => _projectRepository.GetByIdAsync(project.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenProjectDoesNotExist_ReturnsNotFoundFailureResult()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var command = new MarkProjectAsOngoingCommand(projectId);

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
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var project = Domain.Entities.Project.Create(
            "Title",
            "Description",
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 6, 1),
            null,
            null
        ).Value!;
        var command = new MarkProjectAsOngoingCommand(project.Id);

        A.CallTo(() => _projectRepository.GetByIdAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
