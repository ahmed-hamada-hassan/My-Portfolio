using System.Text;
using Application.Features.CV.Query;
using Application.Features.Site_Settings.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.CV.Handlers_Tests;

public class DownloadCVHandlerTests
{
    private readonly ISiteSettingsQueryRepository _siteSettingsQueryRepository;
    private readonly IDocumentStorageService _documentStorageService;
    private readonly ILogger<DownloadCVHandler> _logger;
    private readonly DownloadCVHandler _handler;

    public DownloadCVHandlerTests()
    {
        _siteSettingsQueryRepository = A.Fake<ISiteSettingsQueryRepository>();
        _documentStorageService = A.Fake<IDocumentStorageService>();
        _logger = A.Fake<ILogger<DownloadCVHandler>>();

        _handler = new DownloadCVHandler(
            _siteSettingsQueryRepository,
            _documentStorageService,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WhenSiteSettingsDoNotExist_ReturnsFailureResult()
    {
        // Arrange
        A.CallTo(() => _siteSettingsQueryRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns((SiteSettingsResponse?)null);

        var query = new DownloadCVQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SiteSettingsErrors.SiteSettingsNotFound);

        A.CallTo(() => _siteSettingsQueryRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentStorageService.OpenReadAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WhenCvUrlIsNullOrWhitespace_ReturnsFailureResult(string? cvUrl)
    {
        // Arrange
        var siteSettings = new SiteSettingsResponse(
            SiteTitle: "Portfolio",
            FullName: "John Doe",
            JobTitle: "Software Engineer",
            ProfileImageURL: "https://example.com/pic.jpg",
            CVURL: cvUrl,
            AvailableForWork: true
        );

        A.CallTo(() => _siteSettingsQueryRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(siteSettings);

        var query = new DownloadCVQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SiteSettingsErrors.CVNotFound);

        A.CallTo(() => _siteSettingsQueryRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentStorageService.OpenReadAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDocumentStorageFailsToOpenCv_ReturnsFailureResult()
    {
        // Arrange
        var cvUrl = "documents/cv.pdf";
        var siteSettings = new SiteSettingsResponse(
            SiteTitle: "Portfolio",
            FullName: "John Doe",
            JobTitle: "Software Engineer",
            ProfileImageURL: "https://example.com/pic.jpg",
            CVURL: cvUrl,
            AvailableForWork: true
        );

        var storageError = new Error("Storage.Error", "File not accessible");

        A.CallTo(() => _siteSettingsQueryRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(siteSettings);
        A.CallTo(() => _documentStorageService.OpenReadAsync(cvUrl, A<CancellationToken>._))
            .Returns(Result<Stream>.Failure(storageError));

        var query = new DownloadCVQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(storageError);

        A.CallTo(() => _siteSettingsQueryRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentStorageService.OpenReadAsync(cvUrl, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenCvExistsAndStorageOpensSuccessfully_ReturnsSuccessResultWithStream()
    {
        // Arrange
        var cvUrl = "documents/cv.pdf";
        var siteSettings = new SiteSettingsResponse(
            SiteTitle: "Portfolio",
            FullName: "John Doe",
            JobTitle: "Software Engineer",
            ProfileImageURL: "https://example.com/pic.jpg",
            CVURL: cvUrl,
            AvailableForWork: true
        );

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("fake cv content"));

        A.CallTo(() => _siteSettingsQueryRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(siteSettings);
        A.CallTo(() => _documentStorageService.OpenReadAsync(cvUrl, A<CancellationToken>._))
            .Returns(Result<Stream>.Success(memoryStream));

        var query = new DownloadCVQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(memoryStream);

        A.CallTo(() => _siteSettingsQueryRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentStorageService.OpenReadAsync(cvUrl, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
