using Application.Features.Site_Settings.Command;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Site_Settings.Handlers_Tests;

public class UpdateSiteSettingsHandlerTests
{
    private readonly ISiteSettingsCommandRepository _siteSettingsRepository;
    private readonly ILogger<UpdateSiteSettingsHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateSiteSettingsHandler _handler;

    public UpdateSiteSettingsHandlerTests()
    {
        _siteSettingsRepository = A.Fake<ISiteSettingsCommandRepository>();
        _logger = A.Fake<ILogger<UpdateSiteSettingsHandler>>();
        _unitOfWork = A.Fake<IUnitOfWork>();

        _handler = new UpdateSiteSettingsHandler(
            _siteSettingsRepository,
            _logger,
            _unitOfWork
        );
    }

    [Fact]
    public async Task Handle_WithValidData_UpdatesSettingsAndReturnsSuccess()
    {
        // Arrange
        var settings = CreateSiteSettings();
        var command = new UpdateSiteSettingsCommand(
            SiteTitle: "New Portfolio",
            FullName: "Jane Doe",
            JobTitle: "Lead Architect",
            AvailableForWork: false
        );

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        settings.SiteTitle.Should().Be("New Portfolio");
        settings.FullName.Should().Be("Jane Doe");
        settings.JobTitle.Should().Be("Lead Architect");
        settings.AvailableForWork.Should().BeFalse();

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenSiteSettingsNotFound_ReturnsFailureResult()
    {
        // Arrange
        var command = new UpdateSiteSettingsCommand("New Title", "Jane Doe", "Lead Architect", false);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns((SiteSettings?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SiteSettingsErrors.SiteSettingsNotFound);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var settings = CreateSiteSettings();
        var command = new UpdateSiteSettingsCommand(
            SiteTitle: "   ",
            FullName: null,
            JobTitle: null,
            AvailableForWork: null
        );

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SiteSettingsErrors.InvalidSiteTitle);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var settings = CreateSiteSettings();
        var command = new UpdateSiteSettingsCommand("New Title", "Jane Doe", "Lead Architect", false);

        A.CallTo(() => _siteSettingsRepository.GetSiteSettingsAsync(A<CancellationToken>._))
            .Returns(settings);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    private static SiteSettings CreateSiteSettings()
    {
        return SiteSettings.Create(
            "Portfolio",
            "John Doe",
            "Senior Engineer",
            "https://example.com/pic.jpg",
            "pub-id-123",
            "documents/cv.pdf",
            true
        ).Value!;
    }
}
