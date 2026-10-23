using Application.Features.Skill.Command;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Skill.Handlers_Tests;

public class DeleteSkillHandlerTests
{
    private readonly ISkillCommandRepository _skillRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteSkillHandler> _logger;
    private readonly DeleteSkillHandler _handler;

    public DeleteSkillHandlerTests()
    {
        _skillRepository = A.Fake<ISkillCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<DeleteSkillHandler>>();

        _handler = new DeleteSkillHandler(
            _skillRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WhenSkillExistsAndNotUsed_DeletesSkillAndReturnsSuccess()
    {
        // Arrange
        var skill = Domain.Entities.Skill.Create("C#", SkillType.Language, "csharp.svg").Value!;
        var command = new DeleteSkillCommand(skill.Id);

        A.CallTo(() => _skillRepository.GetByIdAsync(skill.Id, A<CancellationToken>._))
            .Returns(skill);
        A.CallTo(() => _skillRepository.IsUsedAsync(skill.Id, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _skillRepository.GetByIdAsync(skill.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _skillRepository.IsUsedAsync(skill.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _skillRepository.Remove(skill))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenSkillDoesNotExist_ReturnsFailureResult()
    {
        // Arrange
        var skillId = Guid.NewGuid();
        var command = new DeleteSkillCommand(skillId);

        A.CallTo(() => _skillRepository.GetByIdAsync(skillId, A<CancellationToken>._))
            .Returns((Domain.Entities.Skill?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SkillErrors.NotFound);

        A.CallTo(() => _skillRepository.GetByIdAsync(skillId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _skillRepository.IsUsedAsync(A<Guid>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _skillRepository.Remove(A<Domain.Entities.Skill>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenSkillIsUsed_ReturnsFailureResult()
    {
        // Arrange
        var skill = Domain.Entities.Skill.Create("C#", SkillType.Language, "csharp.svg").Value!;
        var command = new DeleteSkillCommand(skill.Id);

        A.CallTo(() => _skillRepository.GetByIdAsync(skill.Id, A<CancellationToken>._))
            .Returns(skill);
        A.CallTo(() => _skillRepository.IsUsedAsync(skill.Id, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SkillErrors.SkillIsUsed);

        A.CallTo(() => _skillRepository.GetByIdAsync(skill.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _skillRepository.IsUsedAsync(skill.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _skillRepository.Remove(A<Domain.Entities.Skill>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var skill = Domain.Entities.Skill.Create("C#", SkillType.Language, "csharp.svg").Value!;
        var command = new DeleteSkillCommand(skill.Id);

        A.CallTo(() => _skillRepository.GetByIdAsync(skill.Id, A<CancellationToken>._))
            .Returns(skill);
        A.CallTo(() => _skillRepository.IsUsedAsync(skill.Id, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _skillRepository.Remove(skill))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
