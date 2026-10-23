using Application.Features.Site_Settings.DTOs;
using Application.Features.Site_Settings.Query;
using Application.Interfaces.Repositories;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Site_Settings.Handlers_Tests;

public class GetSiteSettingsHandlerTests
{
    private readonly ISiteSettingsQueryRepository _siteSettingsRepository;
    private readonly GetSiteSettingsHandler _handler;

    public GetSiteSettingsHandlerTests()
    {
        _siteSettingsRepository = A.Fake<ISiteSettingsQueryRepository>();
        _handler = new GetSiteSettingsHandler(_siteSettingsRepository);
    }

    [Fact]
    public async Task Handle_WhenSettingsExist_ReturnsSuccessResultWithResponse()
    {
        // Arrange
        var settingsResponse = new SiteSettingsResponse(
            SiteTitle: "Portfolio",
            FullName: "John Doe",
            JobTitle: "Software Engineer",
            ProfileImageURL: "https://example.com/pic.jpg",
            CVURL: "documents/cv.pdf",
            AvailableForWork: true
        );

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settingsResponse);

        var query = new GetSiteSettingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(settingsResponse);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenSettingsDoNotExist_ReturnsFailureResult()
    {
        // Arrange
        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns((SiteSettingsResponse?)null);

        var query = new GetSiteSettingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SiteSettingsErrors.SiteSettingsNotFound);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
