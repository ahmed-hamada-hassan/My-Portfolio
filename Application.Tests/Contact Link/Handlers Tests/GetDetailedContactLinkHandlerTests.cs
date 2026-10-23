using Application.Features.Contact_Links.DTOs;
using Application.Features.Contact_Links.Query;
using Application.Interfaces.Repositories;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Contact_Link.Handlers_Tests;

public class GetDetailedContactLinkHandlerTests
{
    private readonly IContactLinkQueryRepository _contactLinkQueryRepository;
    private readonly GetDetailedContactLinkHandler _handler;

    public GetDetailedContactLinkHandlerTests()
    {
        _contactLinkQueryRepository = A.Fake<IContactLinkQueryRepository>();
        _handler = new GetDetailedContactLinkHandler(_contactLinkQueryRepository);
    }

    [Fact]
    public async Task Handle_WhenContactLinkExists_ReturnsSuccessResultWithDetails()
    {
        // Arrange
        var id = Guid.NewGuid();
        var query = new GetDetailedContactLinkQuery(id);
        var expectedDto = new GetDetailsContactLinkDTO(id, "LinkedIn", "https://linkedin.com", 1, true);

        A.CallTo(() => _contactLinkQueryRepository.GetDetailsByIdAsync(id, A<CancellationToken>._))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);

        A.CallTo(() => _contactLinkQueryRepository.GetDetailsByIdAsync(id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenContactLinkDoesNotExist_ReturnsNotFoundFailureResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var query = new GetDetailedContactLinkQuery(id);

        A.CallTo(() => _contactLinkQueryRepository.GetDetailsByIdAsync(id, A<CancellationToken>._))
            .Returns((GetDetailsContactLinkDTO?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContactLinkErrors.SocialLinkNotFound);

        A.CallTo(() => _contactLinkQueryRepository.GetDetailsByIdAsync(id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
