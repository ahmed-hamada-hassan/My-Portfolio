using Application.Features.Site_Settings.Command;
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

namespace Application.Tests.Site_Settings.Handlers_Tests;

public class UpdateProfileImageHandlerTests
{
    private readonly ISiteSettingsCommandRepository _siteSettingsRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProfileImageHandler> _logger;
    private readonly UpdateProfileImageHandler _handler;

    public UpdateProfileImageHandlerTests()
    {
        _siteSettingsRepository = A.Fake<ISiteSettingsCommandRepository>();
        _imageStorageService = A.Fake<IImageStorageService>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<UpdateProfileImageHandler>>();

        _handler = new UpdateProfileImageHandler(
            _siteSettingsRepository,
            _imageStorageService,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidDataAndOldImage_ReturnsSuccessResultAndDeletesOldImage()
    {
        // Arrange
        var settings = CreateSiteSettings("https://example.com/old-profile.jpg", "old-pub-id");
        var fakeImage = CreateFakeImageFile();
        var newImageUrl = "https://example.com/new-profile.jpg";
        var newPublicId = "new-pub-id";
        var command = new UpdateProfileImageCommand(fakeImage);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImage, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success((newImageUrl, newPublicId)));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);
        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/old-profile.jpg", "old-pub-id"))
            .Returns(Result<bool>.Success(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        settings.ProfileImageURL.Should().Be(newImageUrl);
        settings.ProfileImagePublicId.Should().Be(newPublicId);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImage, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync("https://example.com/old-profile.jpg", "old-pub-id"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WithValidDataAndNoOldImage_ReturnsSuccessResultAndDoesNotCallDelete()
    {
        // Arrange
        var settings = CreateSiteSettings(null, null);
        var fakeImage = CreateFakeImageFile();
        var newImageUrl = "https://example.com/new-profile.jpg";
        var newPublicId = "new-pub-id";
        var command = new UpdateProfileImageCommand(fakeImage);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImage, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success((newImageUrl, newPublicId)));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        settings.ProfileImageURL.Should().Be(newImageUrl);
        settings.ProfileImagePublicId.Should().Be(newPublicId);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImage, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenSiteSettingsNotFound_ReturnsFailureResult()
    {
        // Arrange
        var fakeImage = CreateFakeImageFile();
        var command = new UpdateProfileImageCommand(fakeImage);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns((SiteSettings?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SiteSettingsErrors.SiteSettingsNotFound);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(A<IFormFile>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenImageUploadFails_ReturnsFailureResult()
    {
        // Arrange
        var settings = CreateSiteSettings("https://example.com/old.jpg", "old-pub-id");
        var fakeImage = CreateFakeImageFile();
        var uploadError = new Error("UploadFailed", "Failed to upload image.");
        var command = new UpdateProfileImageCommand(fakeImage);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImage, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Failure(uploadError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(uploadError);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImage, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_RollsBackNewImageAndThrowsException()
    {
        // Arrange
        var settings = CreateSiteSettings("https://example.com/old.jpg", "old-pub-id");
        var fakeImage = CreateFakeImageFile();
        var newImageUrl = "https://example.com/new-profile.jpg";
        var newPublicId = "new-pub-id";
        var command = new UpdateProfileImageCommand(fakeImage);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImage, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success((newImageUrl, newPublicId)));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));
        A.CallTo(() => _imageStorageService.DeleteImageAsync(newImageUrl, newPublicId))
            .Returns(Result<bool>.Success(true));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImage, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(newImageUrl, newPublicId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsAndRollbackFails_LogsErrorAndThrowsOriginalDatabaseException()
    {
        // Arrange
        var settings = CreateSiteSettings("https://example.com/old.jpg", "old-pub-id");
        var fakeImage = CreateFakeImageFile();
        var newImageUrl = "https://example.com/new-profile.jpg";
        var newPublicId = "new-pub-id";
        var command = new UpdateProfileImageCommand(fakeImage);
        var cleanupError = new Error("DeleteFailed", "Could not delete uploaded image.");

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImage, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success((newImageUrl, newPublicId)));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));
        A.CallTo(() => _imageStorageService.DeleteImageAsync(newImageUrl, newPublicId))
            .Returns(Result<bool>.Failure(cleanupError));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _imageStorageService.DeleteImageAsync(newImageUrl, newPublicId))
            .MustHaveHappenedOnceExactly();

        A.CallTo(_logger).Where(call =>
            call.Method.Name == "Log" &&
            call.GetArgument<LogLevel>(0) == LogLevel.Error
        ).MustHaveHappened();
    }

    private static SiteSettings CreateSiteSettings(string? profileImageUrl, string? profileImagePublicId)
    {
        return SiteSettings.Create(
            "Portfolio",
            "John Doe",
            "Software Engineer",
            profileImageUrl,
            profileImagePublicId,
            "documents/cv.pdf",
            true
        ).Value!;
    }

    private static IFormFile CreateFakeImageFile(
        string fileName = "profile.jpg",
        long length = 1024,
        string contentType = "image/jpeg")
    {
        var file = A.Fake<IFormFile>();
        A.CallTo(() => file.FileName).Returns(fileName);
        A.CallTo(() => file.Length).Returns(length);
        A.CallTo(() => file.ContentType).Returns(contentType);
        return file;
    }
}
