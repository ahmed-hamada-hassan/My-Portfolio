using Application.Features.Project_Images.Command;
using Application.Features.Project_Images.DTOs;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Project_Images.Handlers_Tests;

public class AddProjectImagesHandlerTests
{
    private readonly IProjectCommandRepository _projectRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddProjectImagesHandler> _logger;
    private readonly AddProjectImagesHandler _handler;

    public AddProjectImagesHandlerTests()
    {
        _projectRepository = A.Fake<IProjectCommandRepository>();
        _imageStorageService = A.Fake<IImageStorageService>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<AddProjectImagesHandler>>();

        _handler = new AddProjectImagesHandler(
            _projectRepository,
            _imageStorageService,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_UploadsImagesAndAddsToProjectAndSaves()
    {
        // Arrange
        var project = CreateProject();
        var fakeFile = CreateFakeImage();
        var images = new List<AddProjectImageDTO>
        {
            new(fakeFile, "Alt Text", 1, true)
        };
        var command = new AddProjectImagesCommand(project.Id, images);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/new.png", "pub-new")));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Images.Should().HaveCount(1);
        project.Images.First().URL.Should().Be("https://example.com/new.png");

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsFailureResult()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var command = new AddProjectImagesCommand(projectId, new List<AddProjectImageDTO>());

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
    public async Task Handle_WhenTotalImagesExceeds8_ReturnsFailureResult()
    {
        // Arrange
        var project = CreateProject();
        // Add 7 images to project
        for (byte i = 1; i <= 7; i++)
        {
            project.AddProjectImage($"https://example.com/{i}.png", $"pub-{i}", "Alt", i, i == 1);
        }

        var fakeFile1 = CreateFakeImage("img1.png");
        var fakeFile2 = CreateFakeImage("img2.png");
        var imagesToAdd = new List<AddProjectImageDTO>
        {
            new(fakeFile1, "Alt 8", 8, false),
            new(fakeFile2, "Alt 9", 9, false) // 7 + 2 = 9 > 8!
        };
        var command = new AddProjectImagesCommand(project.Id, imagesToAdd);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectImageErrors.MaxImagesExceeded);

        A.CallTo(() => _imageStorageService.UploadImageAsync(A<IFormFile>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenImageUploadFails_CleansUpAndReturnsFailureResult()
    {
        // Arrange
        var project = CreateProject();
        var fakeFile1 = CreateFakeImage("img1.png");
        var fakeFile2 = CreateFakeImage("img2.png");
        var images = new List<AddProjectImageDTO>
        {
            new(fakeFile1, "Alt 1", 1, true),
            new(fakeFile2, "Alt 2", 2, false)
        };
        var command = new AddProjectImagesCommand(project.Id, images);
        var uploadError = new Error("UploadFailed", "Failed to upload image.");

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile1, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/1.png", "pub-1")));
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile2, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Failure(uploadError));
        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/1.png", "pub-1"))
            .Returns(Result<bool>.Success(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(uploadError);

        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/1.png", "pub-1"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_CleansUpAndThrowsException()
    {
        // Arrange
        var project = CreateProject();
        var fakeFile = CreateFakeImage();
        var images = new List<AddProjectImageDTO>
        {
            new(fakeFile, "Alt", 1, true)
        };
        var command = new AddProjectImagesCommand(project.Id, images);

        A.CallTo(() => _projectRepository.GetByIdWithImagesAsync(project.Id, A<CancellationToken>._))
            .Returns(project);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/new.png", "pub-new")));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));
        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/new.png", "pub-new"))
            .Returns(Result<bool>.Success(true));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/new.png", "pub-new"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    private static Domain.Entities.Project CreateProject()
    {
        return Domain.Entities.Project.Create(
            "Project Title",
            "Project Description",
            new DateOnly(2025, 1, 1),
            null,
            null,
            null
        ).Value!;
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
