using Application.Features.Contact_Links.DTOs;
using Application.Features.Contact_Links.Query;
using Application.Interfaces.Repositories;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Contact_Link.Handlers_Tests;

public class GetContactLinksHandlerTests
{
    private readonly IContactLinkQueryRepository _contactLinkQueryRepository;
    private readonly GetContactLinksHandler _handler;

    public GetContactLinksHandlerTests()
    {
        _contactLinkQueryRepository = A.Fake<IContactLinkQueryRepository>();
        _handler = new GetContactLinksHandler(_contactLinkQueryRepository);
    }

    [Fact]
    public async Task Handle_WhenContactLinksExist_ReturnsSuccessResultWithList()
    {
        // Arrange
        var query = new GetContactLinksQuery();
        var expectedLinks = new List<GetContactLinkDTO>
        {
            new("GitHub", "https://github.com/user"),
            new("LinkedIn", "https://linkedin.com/in/user")
        };

        A.CallTo(() => _contactLinkQueryRepository.GetAllAsync(A<CancellationToken>._))
            .Returns(expectedLinks);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedLinks);

        A.CallTo(() => _contactLinkQueryRepository.GetAllAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenNoContactLinksExist_ReturnsSuccessResultWithEmptyList()
    {
        // Arrange
        var query = new GetContactLinksQuery();
        var emptyList = new List<GetContactLinkDTO>();

        A.CallTo(() => _contactLinkQueryRepository.GetAllAsync(A<CancellationToken>._))
            .Returns(emptyList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        A.CallTo(() => _contactLinkQueryRepository.GetAllAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
