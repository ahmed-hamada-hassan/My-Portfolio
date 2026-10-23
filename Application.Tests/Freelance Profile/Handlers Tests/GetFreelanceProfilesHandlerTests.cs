using Application.Features.Freelance_Profile.DTOs;
using Application.Features.Freelance_Profile.Query;
using Application.Interfaces.Repositories;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Freelance_Profile.Handlers_Tests;

public class GetFreelanceProfilesHandlerTests
{
    private readonly IFreelanceProfileQueryRepository _freelanceProfileRepository;
    private readonly GetFreelanceProfilesHandler _handler;

    public GetFreelanceProfilesHandlerTests()
    {
        _freelanceProfileRepository = A.Fake<IFreelanceProfileQueryRepository>();
        _handler = new GetFreelanceProfilesHandler(_freelanceProfileRepository);
    }

    [Fact]
    public async Task Handle_WhenProfilesExist_ReturnsListOfProfiles()
    {
        // Arrange
        var profiles = new List<GetFreelanceProfileDTO>
        {
            new(PlatformName: "Upwork", ProfileURL: "https://upwork.com", Icon: "https://cdn.example.com/upwork.svg"),
            new(PlatformName: "Fiverr", ProfileURL: "https://fiverr.com", Icon: "https://cdn.example.com/fiverr.svg")
        };

        A.CallTo(() => _freelanceProfileRepository.GetAllAsync(A<CancellationToken>._))
            .Returns(profiles);

        var query = new GetFreelanceProfilesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(profiles);

        A.CallTo(() => _freelanceProfileRepository.GetAllAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenNoProfilesExist_ReturnsEmptyList()
    {
        // Arrange
        var profiles = new List<GetFreelanceProfileDTO>();

        A.CallTo(() => _freelanceProfileRepository.GetAllAsync(A<CancellationToken>._))
            .Returns(profiles);

        var query = new GetFreelanceProfilesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        A.CallTo(() => _freelanceProfileRepository.GetAllAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
