using Application.Features.Experience.DTOs;
using Application.Features.Experience.Query;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Experience.Handlers_Tests;

public class GetExperienceHandlerTests
{
    private readonly IExperienceQueryRepository _experienceRepository;
    private readonly GetExperienceHandler _handler;

    public GetExperienceHandlerTests()
    {
        _experienceRepository = A.Fake<IExperienceQueryRepository>();
        _handler = new GetExperienceHandler(_experienceRepository);
    }

    [Fact]
    public async Task Handle_WhenExperienceExists_ReturnsSuccessResultWithDetails()
    {
        // Arrange
        var id = Guid.NewGuid();
        var query = new GetExperienceQuery(id);
        var expectedDto = new GetExperienceDetailsDTO(
            id,
            "Tech Corp",
            "Software Engineer",
            "Built web apps",
            ExperienceType.FullTime,
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        );

        A.CallTo(() => _experienceRepository.GetByIdAsync(id, A<CancellationToken>._))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);

        A.CallTo(() => _experienceRepository.GetByIdAsync(id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenExperienceNotFound_ReturnsNotFoundFailureResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var query = new GetExperienceQuery(id);

        A.CallTo(() => _experienceRepository.GetByIdAsync(id, A<CancellationToken>._))
            .Returns((GetExperienceDetailsDTO?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ExperienceErrors.NotFound);

        A.CallTo(() => _experienceRepository.GetByIdAsync(id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
