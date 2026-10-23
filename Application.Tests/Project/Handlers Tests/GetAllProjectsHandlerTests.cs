using Application.Features.Project.DTOs;
using Application.Features.Project.Queries;
using Application.Features.Skill.DTO;
using Application.Interfaces.Repositories;
using Application.Shared;
using Domain.Enums;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Project.Handlers_Tests;

public class GetAllProjectsHandlerTests
{
    private readonly IProjectQueryRepository _projectRepository;
    private readonly GetAllProjectsHandler _handler;

    public GetAllProjectsHandlerTests()
    {
        _projectRepository = A.Fake<IProjectQueryRepository>();
        _handler = new GetAllProjectsHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_WhenProjectsExist_ReturnsPagedResult()
    {
        // Arrange
        var items = new List<ProjectListItemResponse>
        {
            new(
                ProjectId: Guid.NewGuid(),
                Title: "Project 1",
                Slug: "project-1",
                Description: "Desc",
                StartDate: new DateOnly(2025, 1, 1),
                EndDate: null,
                LiveURL: "https://live.example.com",
                GitHubURL: "https://github.com/repo",
                CoverImageURL: "https://example.com/cover.png",
                Skills: new List<SkillResponseDTO> { new(Guid.NewGuid(), "C#", SkillType.Language, "csharp.svg") }
            )
        };
        var pagedResult = new PagedResult<ProjectListItemResponse>(items, 1, 10, 1);

        A.CallTo(() => _projectRepository.GetAllAsync(1, 10, A<CancellationToken>._))
            .Returns(pagedResult);

        var query = new GetAllProjectsQuery(1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(pagedResult);

        A.CallTo(() => _projectRepository.GetAllAsync(1, 10, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
