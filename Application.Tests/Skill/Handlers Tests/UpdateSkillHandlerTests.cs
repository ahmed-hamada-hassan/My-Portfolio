using Application.Features.Skill.Command;
using Application.Features.Skill.DTO;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Skill.Handlers_Tests;

public class UpdateSkillHandlerTests
{
    private readonly ISkillCommandRepository _skillRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSkillHandler> _logger;
    private readonly UpdateSkillHandler _handler;

    public UpdateSkillHandlerTests()
    {
        _skillRepository = A.Fake<ISkillCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<UpdateSkillHandler>>();

        _handler = new UpdateSkillHandler(
            _skillRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var skill = Domain.Entities.Skill.Create("C#", SkillType.Language, "csharp.svg").Value!;
        var request = new UpdateSkillDTO("TypeScript", SkillType.Language, "ts.svg");
        var command = new UpdateSkillCommand(skill.Id, request);

        A.CallTo(() => _skillRepository.GetByIdAsync(skill.Id, A<CancellationToken>._))
            .Returns(skill);
        A.CallTo(() => _skillRepository.NameExistsAsync(request.Name!, skill.Id, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        skill.Name.Should().Be("TypeScript");
        skill.Type.Should().Be(SkillType.Language);
        skill.Icon.Should().Be("ts.svg");

        A.CallTo(() => _skillRepository.GetByIdAsync(skill.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _skillRepository.NameExistsAsync(request.Name!, skill.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenSkillDoesNotExist_ReturnsFailureResult()
    {
        // Arrange
        var skillId = Guid.NewGuid();
        var request = new UpdateSkillDTO("TypeScript", null, null);
        var command = new UpdateSkillCommand(skillId, request);

        A.CallTo(() => _skillRepository.GetByIdAsync(skillId, A<CancellationToken>._))
            .Returns((Domain.Entities.Skill?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SkillErrors.NotFound);

        A.CallTo(() => _skillRepository.GetByIdAsync(skillId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var skill = Domain.Entities.Skill.Create("C#", SkillType.Language, "csharp.svg").Value!;
        var request = new UpdateSkillDTO("ExistingName", null, null);
        var command = new UpdateSkillCommand(skill.Id, request);

        A.CallTo(() => _skillRepository.GetByIdAsync(skill.Id, A<CancellationToken>._))
            .Returns(skill);
        A.CallTo(() => _skillRepository.NameExistsAsync(request.Name!, skill.Id, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SkillErrors.SkillAlreadyExist);

        A.CallTo(() => _skillRepository.NameExistsAsync(request.Name!, skill.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var skill = Domain.Entities.Skill.Create("C#", SkillType.Language, "csharp.svg").Value!;
        var request = new UpdateSkillDTO("TypeScript", null, null);
        var command = new UpdateSkillCommand(skill.Id, request);

        A.CallTo(() => _skillRepository.GetByIdAsync(skill.Id, A<CancellationToken>._))
            .Returns(skill);
        A.CallTo(() => _skillRepository.NameExistsAsync(request.Name!, skill.Id, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
