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

public class CreateSkillHandlerTests
{
    private readonly ISkillCommandRepository _skillRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateSkillHandler> _logger;
    private readonly CreateSkillHandler _handler;

    public CreateSkillHandlerTests()
    {
        _skillRepository = A.Fake<ISkillCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<CreateSkillHandler>>();

        _handler = new CreateSkillHandler(
            _skillRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessfulIdResult()
    {
        // Arrange
        var request = new CreateSkillDTO(
            Name: "C#",
            Type: SkillType.Language,
            Icon: "https://cdn.example.com/csharp.svg"
        );
        var command = new CreateSkillCommand(request);

        A.CallTo(() => _skillRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _skillRepository.AddAsync(A<Domain.Entities.Skill>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        A.CallTo(() => _skillRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _skillRepository.AddAsync(A<Domain.Entities.Skill>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreateSkillDTO(
            Name: "C#",
            Type: SkillType.Language,
            Icon: "https://cdn.example.com/csharp.svg"
        );
        var command = new CreateSkillCommand(request);

        A.CallTo(() => _skillRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SkillErrors.SkillAlreadyExist);

        A.CallTo(() => _skillRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _skillRepository.AddAsync(A<Domain.Entities.Skill>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreateSkillDTO(
            Name: "",
            Type: SkillType.Language,
            Icon: "https://cdn.example.com/csharp.svg"
        );
        var command = new CreateSkillCommand(request);

        A.CallTo(() => _skillRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SkillErrors.InvalidName);

        A.CallTo(() => _skillRepository.AddAsync(A<Domain.Entities.Skill>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var request = new CreateSkillDTO(
            Name: "C#",
            Type: SkillType.Language,
            Icon: "https://cdn.example.com/csharp.svg"
        );
        var command = new CreateSkillCommand(request);

        A.CallTo(() => _skillRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _skillRepository.AddAsync(A<Domain.Entities.Skill>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _skillRepository.AddAsync(A<Domain.Entities.Skill>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
