using Application.Features.Freelance_Profile.DTOs;
using Application.Features.Freelance_Profile.Query;
using Application.Interfaces.Repositories;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Freelance_Profile.Handlers_Tests;

public class GetDetailedFreelanceProfilesHandlerTests
{
    private readonly IFreelanceProfileQueryRepository _freelanceProfileRepository;
    private readonly GetDetailedFreelanceProfilesHandler _handler;

    public GetDetailedFreelanceProfilesHandlerTests()
    {
        _freelanceProfileRepository = A.Fake<IFreelanceProfileQueryRepository>();
        _handler = new GetDetailedFreelanceProfilesHandler(_freelanceProfileRepository);
    }

    [Fact]
    public async Task Handle_WhenProfilesExist_ReturnsListOfDetailedProfiles()
    {
        // Arrange
        var profiles = new List<GetDetailsFreelanceProfileDTO>
        {
            new(Guid.NewGuid(), "Upwork", "https://upwork.com", "https://cdn.example.com/upwork.svg", 1, true),
            new(Guid.NewGuid(), "Fiverr", "https://fiverr.com", "https://cdn.example.com/fiverr.svg", 2, false)
        };

        A.CallTo(() => _freelanceProfileRepository.GetDetailsAsync(A<CancellationToken>._))
            .Returns(profiles);

        var query = new GetDetailedFreelanceProfilesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(profiles);

        A.CallTo(() => _freelanceProfileRepository.GetDetailsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenNoProfilesExist_ReturnsEmptyList()
    {
        // Arrange
        var profiles = new List<GetDetailsFreelanceProfileDTO>();

        A.CallTo(() => _freelanceProfileRepository.GetDetailsAsync(A<CancellationToken>._))
            .Returns(profiles);

        var query = new GetDetailedFreelanceProfilesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        A.CallTo(() => _freelanceProfileRepository.GetDetailsAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
