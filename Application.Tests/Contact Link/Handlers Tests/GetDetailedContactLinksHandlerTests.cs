using Application.Features.Contact_Links.DTOs;
using Application.Features.Contact_Links.Query;
using Application.Interfaces.Repositories;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Contact_Link.Handlers_Tests;

public class GetDetailedContactLinksHandlerTests
{
    private readonly IContactLinkQueryRepository _contactLinkQueryRepository;
    private readonly GetDetailedContactLinksHandler _handler;

    public GetDetailedContactLinksHandlerTests()
    {
        _contactLinkQueryRepository = A.Fake<IContactLinkQueryRepository>();
        _handler = new GetDetailedContactLinksHandler(_contactLinkQueryRepository);
    }

    [Fact]
    public async Task Handle_WhenContactLinksExist_ReturnsSuccessResultWithList()
    {
        // Arrange
        var query = new GetDetailedContactLinksQuery();
        var expectedLinks = new List<GetDetailsContactLinkDTO>
        {
            new(Guid.NewGuid(), "GitHub", "https://github.com/user", 1, true),
            new(Guid.NewGuid(), "LinkedIn", "https://linkedin.com/in/user", 2, true)
        };

        A.CallTo(() => _contactLinkQueryRepository.GetDetailsAsync(A<CancellationToken>._))
            .Returns(expectedLinks);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedLinks);

        A.CallTo(() => _contactLinkQueryRepository.GetDetailsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenNoContactLinksExist_ReturnsSuccessResultWithEmptyList()
    {
        // Arrange
        var query = new GetDetailedContactLinksQuery();
        var emptyList = new List<GetDetailsContactLinkDTO>();

        A.CallTo(() => _contactLinkQueryRepository.GetDetailsAsync(A<CancellationToken>._))
            .Returns(emptyList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        A.CallTo(() => _contactLinkQueryRepository.GetDetailsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
