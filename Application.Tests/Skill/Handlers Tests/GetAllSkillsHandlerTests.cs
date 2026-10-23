using Application.Features.Skill.DTO;
using Application.Features.Skill.Query;
using Application.Interfaces.Repositories;
using Domain.Enums;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Skill.Handlers_Tests;

public class GetAllSkillsHandlerTests
{
    private readonly ISkillQueryRepository _skillQueryRepository;
    private readonly GetAllSkillsHandler _handler;

    public GetAllSkillsHandlerTests()
    {
        _skillQueryRepository = A.Fake<ISkillQueryRepository>();
        _handler = new GetAllSkillsHandler(_skillQueryRepository);
    }

    [Fact]
    public async Task Handle_WhenTypeIsNull_QueriesAllSkillsAndReturnsList()
    {
        // Arrange
        var skills = new List<SkillResponseDTO>
        {
            new(Guid.NewGuid(), "C#", SkillType.Language, "csharp.svg"),
            new(Guid.NewGuid(), ".NET", SkillType.Framework, "dotnet.svg")
        };

        A.CallTo(() => _skillQueryRepository.GetAllAsync(null, A<CancellationToken>._))
            .Returns(skills);

        var query = new GetAllSkillsQuery(null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(skills);

        A.CallTo(() => _skillQueryRepository.GetAllAsync(null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenTypeIsProvided_QueriesSkillsByTypeAndReturnsList()
    {
        // Arrange
        var skills = new List<SkillResponseDTO>
        {
            new(Guid.NewGuid(), "C#", SkillType.Language, "csharp.svg")
        };

        A.CallTo(() => _skillQueryRepository.GetAllAsync(SkillType.Language, A<CancellationToken>._))
            .Returns(skills);

        var query = new GetAllSkillsQuery(SkillType.Language);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(skills);

        A.CallTo(() => _skillQueryRepository.GetAllAsync(SkillType.Language, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenNoSkillsExist_ReturnsEmptyList()
    {
        // Arrange
        var skills = new List<SkillResponseDTO>();

        A.CallTo(() => _skillQueryRepository.GetAllAsync(null, A<CancellationToken>._))
            .Returns(skills);

        var query = new GetAllSkillsQuery(null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        A.CallTo(() => _skillQueryRepository.GetAllAsync(null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
