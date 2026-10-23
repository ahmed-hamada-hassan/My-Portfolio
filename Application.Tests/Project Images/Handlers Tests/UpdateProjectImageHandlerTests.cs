using Application.Features.Project_Images.Command;
using Application.Features.Project_Images.DTOs;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Errors;
using Domain.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Project_Images.Handlers_Tests;

public class UpdateProjectImageHandlerTests
{
    private readonly IProjectCommandRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<UpdateProjectImageHandler> _logger;
    private readonly UpdateProjectImageHandler _handler;

    public UpdateProjectImageHandlerTests()
    {
        _projectRepository = A.Fake<IProjectCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _imageStorageService = A.Fake<IImageStorageService>();
        _logger = A.Fake<ILogger<UpdateProjectImageHandler>>();

        _handler = new UpdateProjectImageHandler(
            _projectRepository,
            _unitOfWork,
            _imageStorageService,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithNewImageAndValidData_UploadsNewImageUpdatesAndDeletesOldImage()
    {
        // Arrange
        var project = CreateProjectWithImage("https://example.com/old.png", "old-pub-1", out var imageId);
        var fakeFile = CreateFakeImage("new.png");
        var newUrl = "https://example.com/new.png";
        var newPubId = "new-pub-1";
        var request = new UpdateProjectImageDTO(fakeFile, "New Alt", 2, false);
        var command = new UpdateProjectImageCommand(project.Id, imageId, request);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success((newUrl, newPubId)));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);
        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/old.png", "old-pub-1"))
            .Returns(Result<bool>.Success(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedImage = project.Images.First(i => i.Id == imageId);
        updatedImage.URL.Should().Be(newUrl);
        updatedImage.PublicId.Should().Be(newPubId);
        updatedImage.AltText.Should().Be("New Alt");
        updatedImage.DisplayOrder.Should().Be(2);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/old.png", "old-pub-1"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WithoutNewImageAndValidData_UpdatesAndDoesNotUploadOrDelete()
    {
        // Arrange
        var project = CreateProjectWithImage("https://example.com/old.png", "old-pub-1", out var imageId);
        var request = new UpdateProjectImageDTO(null, "Updated Alt Only", 3, true);
        var command = new UpdateProjectImageCommand(project.Id, imageId, request);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedImage = project.Images.First(i => i.Id == imageId);
        updatedImage.URL.Should().Be("https://example.com/old.png");
        updatedImage.AltText.Should().Be("Updated Alt Only");
        updatedImage.DisplayOrder.Should().Be(3);

        A.CallTo(() => _imageStorageService.UploadImageAsync(A<IFormFile>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsFailureResult()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var request = new UpdateProjectImageDTO(null, "Alt", 1, true);
        var command = new UpdateProjectImageCommand(projectId, imageId, request);

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
    }

    [Fact]
    public async Task Handle_WhenProjectImageNotFound_ReturnsFailureResult()
    {
        // Arrange
        var project = CreateProjectWithImage("https://example.com/old.png", "old-pub-1", out _);
        var nonExistentImageId = Guid.NewGuid();
        var request = new UpdateProjectImageDTO(null, "Alt", 1, true);
        var command = new UpdateProjectImageCommand(project.Id, nonExistentImageId, request);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectImageErrors.ImageNotFound);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenNewImageUploadFails_ReturnsFailureResult()
    {
        // Arrange
        var project = CreateProjectWithImage("https://example.com/old.png", "old-pub-1", out var imageId);
        var fakeFile = CreateFakeImage("new.png");
        var uploadError = new Error("UploadFailed", "Failed to upload image.");
        var request = new UpdateProjectImageDTO(fakeFile, "Alt", 1, true);
        var command = new UpdateProjectImageCommand(project.Id, imageId, request);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Failure(uploadError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(uploadError);

        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_CleansUpNewImageAndReturnsFailureResult()
    {
        // Arrange
        var project = CreateProjectWithImage("https://example.com/old.png", "old-pub-1", out var imageId);
        var fakeFile = CreateFakeImage("new.png");
        var newUrl = "https://example.com/new.png";
        var newPubId = "new-pub-1";
        // Invalid display order (15 > 8)
        var request = new UpdateProjectImageDTO(fakeFile, "Alt", 15, true);
        var command = new UpdateProjectImageCommand(project.Id, imageId, request);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success((newUrl, newPubId)));
        A.CallTo(() => _imageStorageService.DeleteImageAsync(newUrl, newPubId))
            .Returns(Result<bool>.Success(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectImageErrors.InvalidDisplayOrder);

        A.CallTo(() => _imageStorageService.DeleteImageAsync(newUrl, newPubId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_CleansUpNewImageAndThrowsException()
    {
        // Arrange
        var project = CreateProjectWithImage("https://example.com/old.png", "old-pub-1", out var imageId);
        var fakeFile = CreateFakeImage("new.png");
        var newUrl = "https://example.com/new.png";
        var newPubId = "new-pub-1";
        var request = new UpdateProjectImageDTO(fakeFile, "Alt", 2, true);
        var command = new UpdateProjectImageCommand(project.Id, imageId, request);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success((newUrl, newPubId)));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));
        A.CallTo(() => _imageStorageService.DeleteImageAsync(newUrl, newPubId))
            .Returns(Result<bool>.Success(true));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _imageStorageService.DeleteImageAsync(newUrl, newPubId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
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

    private static IFormFile CreateFakeImage(string fileName = "image.png")
    {
        var file = A.Fake<IFormFile>();
        A.CallTo(() => file.FileName).Returns(fileName);
        A.CallTo(() => file.Length).Returns(1024);
        A.CallTo(() => file.ContentType).Returns("image/png");
        return file;
    }
}
