using Application.Features.Experience.DTOs;
using Application.Features.Experience.Query;
using Application.Interfaces.Repositories;
using Domain.Enums;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Experience.Handlers_Tests;

public class GetExperiencesQueryHandlerTests
{
    private readonly IExperienceQueryRepository _experienceRepository;
    private readonly GetExperiencesQueryHandler _handler;

    public GetExperiencesQueryHandlerTests()
    {
        _experienceRepository = A.Fake<IExperienceQueryRepository>();
        _handler = new GetExperiencesQueryHandler(_experienceRepository);
    }

    [Fact]
    public async Task Handle_WhenExperiencesExist_ReturnsSuccessResultWithList()
    {
        // Arrange
        var query = new GetExperiencesQuery();
        var expectedList = new List<GetExperienceListItemDTO>
        {
            new(
                Guid.NewGuid(),
                "Tech Corp",
                "Senior Dev",
                ExperienceType.FullTime,
                DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
            ),
            new(
                Guid.NewGuid(),
                "Startup Inc",
                "Lead Architect",
                ExperienceType.Freelance,
                DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                null
            )
        };

        A.CallTo(() => _experienceRepository.GetAllAsync(A<CancellationToken>._))
            .Returns(expectedList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedList);

        A.CallTo(() => _experienceRepository.GetAllAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenNoExperiencesExist_ReturnsSuccessResultWithEmptyList()
    {
        // Arrange
        var query = new GetExperiencesQuery();
        var emptyList = new List<GetExperienceListItemDTO>();

        A.CallTo(() => _experienceRepository.GetAllAsync(A<CancellationToken>._))
            .Returns(emptyList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        A.CallTo(() => _experienceRepository.GetAllAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
