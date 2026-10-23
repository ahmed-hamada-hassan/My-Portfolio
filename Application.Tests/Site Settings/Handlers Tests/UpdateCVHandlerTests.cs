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

public class UpdateCVHandlerTests
{
    private readonly ISiteSettingsCommandRepository _siteSettingsRepository;
    private readonly IDocumentStorageService _documentStorageService;
    private readonly ILogger<UpdateCVHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateCVHandler _handler;

    public UpdateCVHandlerTests()
    {
        _siteSettingsRepository = A.Fake<ISiteSettingsCommandRepository>();
        _documentStorageService = A.Fake<IDocumentStorageService>();
        _logger = A.Fake<ILogger<UpdateCVHandler>>();
        _unitOfWork = A.Fake<IUnitOfWork>();

        _handler = new UpdateCVHandler(
            _siteSettingsRepository,
            _documentStorageService,
            _logger,
            _unitOfWork
        );
    }

    [Fact]
    public async Task Handle_WithValidDataAndOldCV_ReturnsSuccessResultAndDeletesOldCV()
    {
        // Arrange
        var settings = CreateSiteSettings("documents/old-cv.pdf");
        var fakeFile = CreateFakeFile("cv.pdf");
        var newCvUrl = "documents/new-cv.pdf";
        var command = new UpdateCVCommand(fakeFile);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);
        A.CallTo(() => _documentStorageService.UploadAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<string>.Success(newCvUrl));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);
        A.CallTo(() => _documentStorageService.DeleteAsync("documents/old-cv.pdf", A<CancellationToken>._))
            .Returns(Result<bool>.Success(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        settings.CVURL.Should().Be(newCvUrl);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentStorageService.UploadAsync(fakeFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentStorageService.DeleteAsync("documents/old-cv.pdf", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WithValidDataAndNoOldCV_ReturnsSuccessResultAndDoesNotCallDelete()
    {
        // Arrange
        var settings = CreateSiteSettings(null);
        var fakeFile = CreateFakeFile("cv.pdf");
        var newCvUrl = "documents/new-cv.pdf";
        var command = new UpdateCVCommand(fakeFile);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);
        A.CallTo(() => _documentStorageService.UploadAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<string>.Success(newCvUrl));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        settings.CVURL.Should().Be(newCvUrl);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentStorageService.UploadAsync(fakeFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentStorageService.DeleteAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenSiteSettingsNotFound_ReturnsFailureResult()
    {
        // Arrange
        var fakeFile = CreateFakeFile("cv.pdf");
        var command = new UpdateCVCommand(fakeFile);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns((SiteSettings?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SiteSettingsErrors.SiteSettingsNotFound);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentStorageService.UploadAsync(A<IFormFile>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenCVUploadFails_ReturnsFailureResult()
    {
        // Arrange
        var settings = CreateSiteSettings("documents/old-cv.pdf");
        var fakeFile = CreateFakeFile("cv.pdf");
        var uploadError = new Error("UploadFailed", "Failed to upload CV document.");
        var command = new UpdateCVCommand(fakeFile);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);
        A.CallTo(() => _documentStorageService.UploadAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<string>.Failure(uploadError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(uploadError);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentStorageService.UploadAsync(fakeFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_RollsBackNewCVAndThrowsException()
    {
        // Arrange
        var settings = CreateSiteSettings("documents/old-cv.pdf");
        var fakeFile = CreateFakeFile("cv.pdf");
        var newCvUrl = "documents/new-cv.pdf";
        var command = new UpdateCVCommand(fakeFile);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);
        A.CallTo(() => _documentStorageService.UploadAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<string>.Success(newCvUrl));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));
        A.CallTo(() => _documentStorageService.DeleteAsync(newCvUrl, CancellationToken.None))
            .Returns(Result<bool>.Success(true));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentStorageService.UploadAsync(fakeFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentStorageService.DeleteAsync(newCvUrl, CancellationToken.None))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsAndRollbackFails_LogsErrorAndThrowsOriginalDatabaseException()
    {
        // Arrange
        var settings = CreateSiteSettings("documents/old-cv.pdf");
        var fakeFile = CreateFakeFile("cv.pdf");
        var newCvUrl = "documents/new-cv.pdf";
        var command = new UpdateCVCommand(fakeFile);
        var cleanupError = new Error("DeleteFailed", "Could not delete uploaded CV.");

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);
        A.CallTo(() => _documentStorageService.UploadAsync(fakeFile, A<CancellationToken>._))
            .Returns(Result<string>.Success(newCvUrl));
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));
        A.CallTo(() => _documentStorageService.DeleteAsync(newCvUrl, CancellationToken.None))
            .Returns(Result<bool>.Failure(cleanupError));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _documentStorageService.DeleteAsync(newCvUrl, CancellationToken.None))
            .MustHaveHappenedOnceExactly();

        A.CallTo(_logger).Where(call =>
            call.Method.Name == "Log" &&
            call.GetArgument<LogLevel>(0) == LogLevel.Error
        ).MustHaveHappened();
    }

    private static SiteSettings CreateSiteSettings(string? cvUrl)
    {
        return SiteSettings.Create(
            "Portfolio",
            "John Doe",
            "Software Engineer",
            "https://example.com/pic.jpg",
            "pub-id",
            cvUrl,
            true
        ).Value!;
    }

    private static IFormFile CreateFakeFile(
        string fileName = "cv.pdf",
        long length = 1024,
        string contentType = "application/pdf")
    {
        var file = A.Fake<IFormFile>();
        A.CallTo(() => file.FileName).Returns(fileName);
        A.CallTo(() => file.Length).Returns(length);
        A.CallTo(() => file.ContentType).Returns(contentType);
        return file;
    }
}
