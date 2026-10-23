using Application.Features.Project.Command;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Project.Handlers_Tests;

public class DeleteProjectHandlerTests
{
    private readonly IProjectCommandRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<DeleteProjectHandler> _logger;
    private readonly DeleteProjectHandler _handler;

    public DeleteProjectHandlerTests()
    {
        _projectRepository = A.Fake<IProjectCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _imageStorageService = A.Fake<IImageStorageService>();
        _logger = A.Fake<ILogger<DeleteProjectHandler>>();

        _handler = new DeleteProjectHandler(
            _projectRepository,
            _unitOfWork,
            _imageStorageService,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WhenProjectExistsWithImages_DeletesImagesAndProjectAndReturnsSuccess()
    {
        // Arrange
        var project = CreateProjectWithImage("https://example.com/img.png", "pub-123");
        var command = new DeleteProjectCommand(project.Id);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/img.png", "pub-123"))
            .Returns(Result<bool>.Success(true));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/img.png", "pub-123"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _projectRepository.Remove(project))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsNotFoundFailureResult()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var command = new DeleteProjectCommand(projectId);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(projectId, A<CancellationToken>._))
            .Returns((Domain.Entities.Project?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.NotFound);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(projectId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _projectRepository.Remove(A<Domain.Entities.Project>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenImageDeletionFails_ReturnsFailureResult()
    {
        // Arrange
        var project = CreateProjectWithImage("https://example.com/img.png", "pub-123");
        var command = new DeleteProjectCommand(project.Id);
        var deleteError = new Error("DeleteFailed", "Failed to delete image.");

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/img.png", "pub-123"))
            .Returns(Result<bool>.Failure(deleteError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(deleteError);

        A.CallTo(() => _projectRepository.Remove(A<Domain.Entities.Project>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var project = CreateProjectWithImage("https://example.com/img.png", "pub-123");
        var command = new DeleteProjectCommand(project.Id);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/img.png", "pub-123"))
            .Returns(Result<bool>.Success(true));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _projectRepository.Remove(project))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    private static Domain.Entities.Project CreateProjectWithImage(string url, string publicId)
    {
        var project = Domain.Entities.Project.Create(
            "Title",
            "Description",
            new DateOnly(2025, 1, 1),
            null,
            null,
            null
        ).Value!;

        project.AddProjectImage(url, publicId, "Alt", 1, true);
        return project;
    }
}
