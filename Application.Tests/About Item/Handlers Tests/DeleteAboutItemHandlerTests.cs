using Application.Features.About_Item.Command;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.About_Item.Handlers_Tests;

public class DeleteAboutItemHandlerTests
{
    private readonly IAboutItemCommandRepository _aboutItemCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAboutItemHandler> _logger;

    private readonly DeleteAboutItemHandler _handler;

    public DeleteAboutItemHandlerTests()
    {
        _aboutItemCommandRepository = A.Fake<IAboutItemCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<DeleteAboutItemHandler>>();

        _handler = new DeleteAboutItemHandler(_aboutItemCommandRepository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessfulResult()
    {
        // Arrange
        var aboutItemId = Guid.NewGuid();
        var command = new DeleteAboutItemCommand(aboutItemId);
        var aboutItem = A.Fake<AboutItem>();
        A.CallTo(() => _aboutItemCommandRepository.GetByIdAsync(aboutItemId, A<CancellationToken>._))
            .Returns(aboutItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _aboutItemCommandRepository.Remove(aboutItem))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WithNonExistentAboutItem_ReturnsAboutItemNotFoundError()
    {
        // Arrange
        var aboutItemId = Guid.NewGuid();
        var command = new DeleteAboutItemCommand(aboutItemId);
        A.CallTo(() => _aboutItemCommandRepository.GetByIdAsync(aboutItemId, A<CancellationToken>._))
            .Returns((AboutItem)null!);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(AboutItemErrors.AboutItemNotFound);

        A.CallTo(() => _aboutItemCommandRepository.Remove(A<AboutItem>._))
            .MustNotHaveHappened();

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenRemoveThrowsException_ThrowsException()
    {
        // Arrange
        var aboutItemId = Guid.NewGuid();

        var command = new DeleteAboutItemCommand(aboutItemId);

        var aboutItem = A.Fake<AboutItem>();
        A.CallTo(() => _aboutItemCommandRepository.GetByIdAsync(aboutItemId, A<CancellationToken>._))
            .Returns(aboutItem);

        A.CallTo(() => _aboutItemCommandRepository.Remove(aboutItem))
            .Throws(new Exception("An error occurred while removing the about item."));

        // Act 
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("An error occurred while removing the about item.");
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrowsException_ThrowsException()
    {
        // Arrange
        var aboutItemId = Guid.NewGuid();

        var command = new DeleteAboutItemCommand(aboutItemId);

        var aboutItem = A.Fake<AboutItem>();
        A.CallTo(() => _aboutItemCommandRepository.GetByIdAsync(aboutItemId, A<CancellationToken>._))
            .Returns(aboutItem);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("An error occurred while saving changes."));
        
        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("An error occurred while saving changes.");
    }
}