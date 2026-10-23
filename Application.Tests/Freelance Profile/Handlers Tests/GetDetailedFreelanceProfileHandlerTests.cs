using Application.Features.Freelance_Profile.DTOs;
using Application.Features.Freelance_Profile.Query;
using Application.Interfaces.Repositories;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Freelance_Profile.Handlers_Tests;

public class GetDetailedFreelanceProfileHandlerTests
{
    private readonly IFreelanceProfileQueryRepository _freelanceProfileRepository;
    private readonly GetDetailedFreelanceProfileHandler _handler;

    public GetDetailedFreelanceProfileHandlerTests()
    {
        _freelanceProfileRepository = A.Fake<IFreelanceProfileQueryRepository>();
        _handler = new GetDetailedFreelanceProfileHandler(_freelanceProfileRepository);
    }

    [Fact]
    public async Task Handle_WhenProfileExists_ReturnsSuccessResultWithDto()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var profileDto = new GetDetailsFreelanceProfileDTO(
            Id: profileId,
            PlatformName: "Upwork",
            ProfileURL: "https://upwork.com/freelancers/~test",
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );

        A.CallTo(() => _freelanceProfileRepository.GetDetailsByIdAsync(profileId, A<CancellationToken>._))
            .Returns(profileDto);

        var query = new GetDetailedFreelanceProfileQuery(profileId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(profileDto);

        A.CallTo(() => _freelanceProfileRepository.GetDetailsByIdAsync(profileId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsFailureResult()
    {
        // Arrange
        var profileId = Guid.NewGuid();

        A.CallTo(() => _freelanceProfileRepository.GetDetailsByIdAsync(profileId, A<CancellationToken>._))
            .Returns((GetDetailsFreelanceProfileDTO?)null);

        var query = new GetDetailedFreelanceProfileQuery(profileId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.FreelanceProfileNotFound);

        A.CallTo(() => _freelanceProfileRepository.GetDetailsByIdAsync(profileId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
