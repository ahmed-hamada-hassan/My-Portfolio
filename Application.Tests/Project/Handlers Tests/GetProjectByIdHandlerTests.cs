using Application.Features.Project.DTOs;
using Application.Features.Project.Queries;
using Application.Features.Skill.DTO;
using Application.Interfaces.Repositories;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Project.Handlers_Tests;

public class GetProjectByIdHandlerTests
{
    private readonly IProjectQueryRepository _projectRepository;
    private readonly GetProjectByIdHandler _handler;

    public GetProjectByIdHandlerTests()
    {
        _projectRepository = A.Fake<IProjectQueryRepository>();
        _handler = new GetProjectByIdHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_WhenProjectExists_ReturnsSuccessResultWithProject()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectDetails = new ProjectDetailsResponse(
            ProjectId: projectId,
            Title: "Project",
            Slug: "project-slug",
            Description: "Desc",
            StartDate: new DateOnly(2025, 1, 1),
            EndDate: null,
            LiveURL: "https://example.com",
            GitHubURL: "https://github.com/repo",
            ProjectImages: new List<ProjectImageResponse>(),
            Skills: new List<SkillResponseDTO>()
        );

        A.CallTo(() => _projectRepository.GetByIdAsync(projectId, A<CancellationToken>._))
            .Returns(projectDetails);

        var query = new GetProjectByIdQuery(projectId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(projectDetails);

        A.CallTo(() => _projectRepository.GetByIdAsync(projectId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsFailureResult()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        A.CallTo(() => _projectRepository.GetByIdAsync(projectId, A<CancellationToken>._))
            .Returns((ProjectDetailsResponse?)null);

        var query = new GetProjectByIdQuery(projectId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.NotFound);

        A.CallTo(() => _projectRepository.GetByIdAsync(projectId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
