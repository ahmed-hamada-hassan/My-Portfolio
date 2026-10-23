using Application.Features.Project.Command;
using Application.Features.Project.DTOs;
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

namespace Application.Tests.Project.Handlers_Tests;

public class CreateProjectHandlerTests
{
    private readonly IProjectCommandRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ISkillCommandRepository _skillRepository;
    private readonly ILogger<CreateProjectHandler> _logger;
    private readonly CreateProjectHandler _handler;

    public CreateProjectHandlerTests()
    {
        _projectRepository = A.Fake<IProjectCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _imageStorageService = A.Fake<IImageStorageService>();
        _skillRepository = A.Fake<ISkillCommandRepository>();
        _logger = A.Fake<ILogger<CreateProjectHandler>>();

        _handler = new CreateProjectHandler(
            _projectRepository,
            _unitOfWork,
            _imageStorageService,
            _skillRepository,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessfulIdResult()
    {
        // Arrange
        var fakeFile = CreateFakeImage();
        var images = new List<AddProjectImageDTO>
        {
            new(fakeFile, "Cover Alt", 1, true)
        };
        var skillIds = new List<Guid> { Guid.NewGuid() };
        var request = new CreateProjectDTO(
            Title: "Portfolio Project",
            Description: "Project Description",
            StartDate: new DateOnly(2025, 1, 1),
            EndDate: new DateOnly(2025, 6, 1),
            LiveURL: "https://live.example.com",
            GitHubURL: "https://github.com/example/repo",
            ProjectImages: images,
            SkillIds: skillIds
        );
        var command = new CreateProjectCommand(request);

        A.CallTo(() => _projectRepository.TitleExistsAsync(request.Title, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _skillRepository.AllExistAsync(request.SkillIds, A<CancellationToken>._))
            .Returns(true);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/img1.png", "pub-1")));
        A.CallTo(() => _projectRepository.AddAsync(A<Domain.Entities.Project>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        A.CallTo(() => _projectRepository.TitleExistsAsync(request.Title, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _skillRepository.AllExistAsync(request.SkillIds, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _projectRepository.AddAsync(A<Domain.Entities.Project>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenTitleAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var request = CreateValidRequest();
        var command = new CreateProjectCommand(request);

        A.CallTo(() => _projectRepository.TitleExistsAsync(request.Title, null, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.DuplicateTitle);

        A.CallTo(() => _projectRepository.TitleExistsAsync(request.Title, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _skillRepository.AllExistAsync(A<IReadOnlyList<Guid>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _projectRepository.AddAsync(A<Domain.Entities.Project>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenOneOrMoreSkillsDoNotExist_ReturnsFailureResult()
    {
        // Arrange
        var request = CreateValidRequest();
        var command = new CreateProjectCommand(request);

        A.CallTo(() => _projectRepository.TitleExistsAsync(request.Title, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _skillRepository.AllExistAsync(request.SkillIds, A<CancellationToken>._))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SkillErrors.NotFound);

        A.CallTo(() => _projectRepository.TitleExistsAsync(request.Title, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _skillRepository.AllExistAsync(request.SkillIds, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _projectRepository.AddAsync(A<Domain.Entities.Project>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var invalidRequest = new CreateProjectDTO(
            Title: "Project",
            Description: "Description",
            StartDate: new DateOnly(2025, 6, 1),
            EndDate: new DateOnly(2025, 1, 1), // Invalid: StartDate > EndDate
            LiveURL: null,
            GitHubURL: "https://github.com/example/repo",
            ProjectImages: new List<AddProjectImageDTO>(),
            SkillIds: new List<Guid> { Guid.NewGuid() }
        );
        var command = new CreateProjectCommand(invalidRequest);

        A.CallTo(() => _projectRepository.TitleExistsAsync(invalidRequest.Title, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _skillRepository.AllExistAsync(invalidRequest.SkillIds, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.InvalidDates);

        A.CallTo(() => _projectRepository.AddAsync(A<Domain.Entities.Project>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenImageUploadFails_CleansUpAndReturnsFailureResult()
    {
        // Arrange
        var fakeFile1 = CreateFakeImage("img1.png");
        var fakeFile2 = CreateFakeImage("img2.png");
        var images = new List<AddProjectImageDTO>
        {
            new(fakeFile1, "Cover", 1, true),
            new(fakeFile2, "Second", 2, false)
        };
        var request = new CreateProjectDTO(
            Title: "Portfolio Project",
            Description: "Project Description",
            StartDate: new DateOnly(2025, 1, 1),
            EndDate: null,
            LiveURL: null,
            GitHubURL: "https://github.com/example/repo",
            ProjectImages: images,
            SkillIds: new List<Guid> { Guid.NewGuid() }
        );
        var command = new CreateProjectCommand(request);
        var uploadError = new Error("UploadFailed", "Upload failed");

        A.CallTo(() => _projectRepository.TitleExistsAsync(request.Title, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _skillRepository.AllExistAsync(request.SkillIds, A<CancellationToken>._))
            .Returns(true);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile1, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/img1.png", "pub-1")));
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile2, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Failure(uploadError));
        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/img1.png", "pub-1"))
            .Returns(Result<bool>.Success(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(uploadError);

        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/img1.png", "pub-1"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _projectRepository.AddAsync(A<Domain.Entities.Project>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_CleansUpImagesAndThrowsException()
    {
        // Arrange
        var fakeFile = CreateFakeImage();
        var images = new List<AddProjectImageDTO>
        {
            new(fakeFile, "Cover Alt", 1, true)
        };
        var request = new CreateProjectDTO(
            Title: "Portfolio Project",
            Description: "Project Description",
            StartDate: new DateOnly(2025, 1, 1),
            EndDate: null,
            LiveURL: null,
            GitHubURL: "https://github.com/example/repo",
            ProjectImages: images,
            SkillIds: new List<Guid> { Guid.NewGuid() }
        );
        var command = new CreateProjectCommand(request);

        A.CallTo(() => _projectRepository.TitleExistsAsync(request.Title, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _skillRepository.AllExistAsync(request.SkillIds, A<CancellationToken>._))
            .Returns(true);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/img1.png", "pub-1")));
        A.CallTo(() => _projectRepository.AddAsync(A<Domain.Entities.Project>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));
        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/img1.png", "pub-1"))
            .Returns(Result<bool>.Success(true));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/img1.png", "pub-1"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    private static CreateProjectDTO CreateValidRequest()
    {
        var fakeFile = CreateFakeImage();
        return new CreateProjectDTO(
            Title: "Portfolio Project",
            Description: "Project Description",
            StartDate: new DateOnly(2025, 1, 1),
            EndDate: new DateOnly(2025, 6, 1),
            LiveURL: "https://live.example.com",
            GitHubURL: "https://github.com/example/repo",
            ProjectImages: new List<AddProjectImageDTO> { new(fakeFile, "Cover", 1, true) },
            SkillIds: new List<Guid> { Guid.NewGuid() }
        );
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
