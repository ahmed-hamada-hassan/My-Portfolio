using Application.Features.Project_Images.Command;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Project_Images.Handlers_Tests;

public class DeleteProjectImageHandlerTests
{
    private readonly IProjectCommandRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<DeleteProjectImageHandler> _logger;
    private readonly DeleteProjectImageHandler _handler;

    public DeleteProjectImageHandlerTests()
    {
        _projectRepository = A.Fake<IProjectCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _imageStorageService = A.Fake<IImageStorageService>();
        _logger = A.Fake<ILogger<DeleteProjectImageHandler>>();

        _handler = new DeleteProjectImageHandler(
            _projectRepository,
            _unitOfWork,
            _imageStorageService,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WhenValid_RemovesFromProjectSavesAndDeletesFromStorage()
    {
        // Arrange
        var imageUrl = "https://example.com/img.png";
        var publicId = "pub-123";
        var project = CreateProjectWithImage(imageUrl, publicId, out var imageId);
        var command = new DeleteProjectImageCommand(project.Id, imageId);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);
        A.CallTo(() => _imageStorageService.DeleteImageAsync(imageUrl, publicId))
            .Returns(Result<bool>.Success(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Images.Should().BeEmpty();

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(imageUrl, publicId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsFailureResult()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var command = new DeleteProjectImageCommand(projectId, imageId);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(projectId, A<CancellationToken>._))
            .Returns((Domain.Entities.Project?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.NotFound);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(projectId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenProjectImageNotFound_ReturnsFailureResult()
    {
        // Arrange
        var project = CreateProjectWithImage("https://example.com/img.png", "pub-123", out _);
        var nonExistentImageId = Guid.NewGuid();
        var command = new DeleteProjectImageCommand(project.Id, nonExistentImageId);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectImageErrors.ImageNotFound);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenStorageDeleteFails_ReturnsFailureResult()
    {
        // Arrange
        var imageUrl = "https://example.com/img.png";
        var publicId = "pub-123";
        var project = CreateProjectWithImage(imageUrl, publicId, out var imageId);
        var command = new DeleteProjectImageCommand(project.Id, imageId);
        var deleteError = new Error("DeleteFailed", "Failed to delete image.");

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);
        A.CallTo(() => _imageStorageService.DeleteImageAsync(imageUrl, publicId))
            .Returns(Result<bool>.Failure(deleteError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(deleteError);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(imageUrl, publicId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var imageUrl = "https://example.com/img.png";
        var publicId = "pub-123";
        var project = CreateProjectWithImage(imageUrl, publicId, out var imageId);
        var command = new DeleteProjectImageCommand(project.Id, imageId);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    private static Domain.Entities.Project CreateProjectWithImage(string url, string publicId, out Guid imageId)
    {
        var project = Domain.Entities.Project.Create(
            "Project Title",
            "Project Description",
            new DateOnly(2025, 1, 1),
            null,
            null,
            null
        ).Value!;

        var image = project.AddProjectImage(url, publicId, "Alt", 1, true).Value!;
        imageId = image.Id;
        return project;
    }
}
