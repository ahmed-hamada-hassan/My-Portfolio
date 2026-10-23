using Application.Features.Project.DTOs;
using Application.Features.Project.Queries;
using Application.Features.Skill.DTO;
using Application.Interfaces.Repositories;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Project.Handlers_Tests;

public class GetProjectBySlugHandlerTests
{
    private readonly IProjectQueryRepository _projectRepository;
    private readonly GetProjectBySlugHandler _handler;

    public GetProjectBySlugHandlerTests()
    {
        _projectRepository = A.Fake<IProjectQueryRepository>();
        _handler = new GetProjectBySlugHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_WhenProjectExists_ReturnsSuccessResultWithProject()
    {
        // Arrange
        var slug = "project-slug";
        var projectDetails = new ProjectDetailsResponse(
            ProjectId: Guid.NewGuid(),
            Title: "Project",
            Slug: slug,
            Description: "Desc",
            StartDate: new DateOnly(2025, 1, 1),
            EndDate: null,
            LiveURL: "https://example.com",
            GitHubURL: "https://github.com/repo",
            ProjectImages: new List<ProjectImageResponse>(),
            Skills: new List<SkillResponseDTO>()
        );

        A.CallTo(() => _projectRepository.GetBySlugAsync(slug, A<CancellationToken>._))
            .Returns(projectDetails);

        var query = new GetProjectBySlugQuery(slug);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(projectDetails);

        A.CallTo(() => _projectRepository.GetBySlugAsync(slug, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsFailureResult()
    {
        // Arrange
        var slug = "non-existent-slug";

        A.CallTo(() => _projectRepository.GetBySlugAsync(slug, A<CancellationToken>._))
            .Returns((ProjectDetailsResponse?)null);

        var query = new GetProjectBySlugQuery(slug);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.NotFound);

        A.CallTo(() => _projectRepository.GetBySlugAsync(slug, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
