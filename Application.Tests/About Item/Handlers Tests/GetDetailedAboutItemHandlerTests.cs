using Application.Features.About_Item.DTOs;
using Application.Features.About_Item.Query;
using Application.Interfaces.Repositories;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.About_Item.Handlers_Tests;

public class GetDetailedAboutItemHandlerTests
{ 
    private readonly IAboutItemQueryRepository _aboutItemQueryRepository;
    private readonly ILogger<GetDetailedAboutItemHandler> _logger;

    private readonly GetDetailedAboutItemHandler _handler;

    public GetDetailedAboutItemHandlerTests()
    {
        _aboutItemQueryRepository = A.Fake<IAboutItemQueryRepository>();
        _logger = A.Fake<ILogger<GetDetailedAboutItemHandler>>();

        _handler = new GetDetailedAboutItemHandler(_aboutItemQueryRepository, _logger);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessfulResult()
    {
        // Arrange
        var aboutItemId = Guid.NewGuid();
        var query = new GetDetailedAboutItemQuery(aboutItemId);
        var expectedAboutItem = new GetDetailsAboutItemDTO(
            aboutItemId,
            "Title",
            "Description",
            1,
            true);
        A.CallTo(() => _aboutItemQueryRepository.GetDetailsByIdAsync(aboutItemId, A<CancellationToken>._))
            .Returns(expectedAboutItem);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedAboutItem);
    }

    [Fact]
    public async Task Handle_WhenAboutItemIsNull_ReturnsAboutItemNotFoundError()
    {
        // Arrange
        var aboutItemId = Guid.NewGuid();
        var query = new GetDetailedAboutItemQuery(aboutItemId);
        A.CallTo(() => _aboutItemQueryRepository.GetDetailsByIdAsync(aboutItemId, A<CancellationToken>._))
            .Returns((GetDetailsAboutItemDTO?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(Domain.Errors.AboutItemErrors.AboutItemNotFound);
    }
}