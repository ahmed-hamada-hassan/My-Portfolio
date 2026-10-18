using Application.Features.About_Item.Command;
using Application.Features.About_Item.DTOs;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.About_Item.Handlers_Tests;

public class UpdateAboutItemHandlerTests
{
    private readonly IAboutItemCommandRepository _aboutItemCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAboutItemHandler> _logger;

    private readonly UpdateAboutItemHandler _handler;

    public UpdateAboutItemHandlerTests()
    {
        _aboutItemCommandRepository = A.Fake<IAboutItemCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<UpdateAboutItemHandler>>();

        _handler = new UpdateAboutItemHandler(_aboutItemCommandRepository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessfulResult()
    {
        // Arrange
        var aboutItemId = Guid.NewGuid();
        var command = new UpdateAboutItemCommand(aboutItemId, new UpdateAboutItemDTO("Updated Title", "Updated Description", 3, false));

        var existingAboutItemResult = AboutItem.Create("Old Title", "Old Description", 2, true);
        var existingAboutItem = existingAboutItemResult.Value!;

        A.CallTo(() => _aboutItemCommandRepository.GetByIdAsync(aboutItemId, A<CancellationToken>._))
            .Returns(existingAboutItem);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        existingAboutItem.Title.Should().Be("Updated Title");
        existingAboutItem.Description.Should().Be("Updated Description");
        existingAboutItem.DisplayOrder.Should().Be(3);
        existingAboutItem.IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNonExistentAboutItem_ReturnsAboutItemNotFoundError()
    {
        // Arrange
        var aboutItemId = Guid.NewGuid();
        var command = new UpdateAboutItemCommand(aboutItemId, new UpdateAboutItemDTO("Updated Title", "Updated Description", 3, false));
        A.CallTo(() => _aboutItemCommandRepository.GetByIdAsync(aboutItemId, A<CancellationToken>._))
            .Returns((Domain.Entities.AboutItem)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().BeEquivalentTo(AboutItemErrors.AboutItemNotFound);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenTitleExists_ReturnsTitleAlreadyExistsError()
    {
        // Arrange
        var aboutItemId = Guid.NewGuid();
        var command = new UpdateAboutItemCommand(aboutItemId, new UpdateAboutItemDTO("Existing Title", "Updated Description", 3, false));

        var existingAboutItemResult = AboutItem.Create("Old Title", "Old Description", 2, true);
        var existingAboutItem = existingAboutItemResult.Value!;

        A.CallTo(() => _aboutItemCommandRepository.GetByIdAsync(aboutItemId, A<CancellationToken>._))
            .Returns(existingAboutItem);
        A.CallTo(() => _aboutItemCommandRepository.TitleExistsAsync("Existing Title", aboutItemId, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(AboutItemErrors.TitleAlreadyExists);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDisplayOrderExists_ReturnsDisplayOrderAlreadyExistsError()
    {
        // Arrange
        var aboutItemId = Guid.NewGuid();
        var command = new UpdateAboutItemCommand(aboutItemId, new UpdateAboutItemDTO("Updated Title", "Updated Description", 3, false));

        var existingAboutItemResult = AboutItem.Create("Old Title", "Old Description", 2, true);
        var existingAboutItem = existingAboutItemResult.Value!;

        A.CallTo(() => _aboutItemCommandRepository.GetByIdAsync(aboutItemId, A<CancellationToken>._))
            .Returns(existingAboutItem);

        A.CallTo(() => _aboutItemCommandRepository.DisplayOrderExistsAsync(3, aboutItemId, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(AboutItemErrors.DisplayOrderAlreadyExists);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }
    
    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ReturnsError()
    {
        // Arrange
        var aboutItemId = Guid.NewGuid();
        var command = new UpdateAboutItemCommand(aboutItemId, new UpdateAboutItemDTO("Updated Title", "Updated Description", 3, false));
        A.CallTo(() => _aboutItemCommandRepository.GetByIdAsync(aboutItemId, A<CancellationToken>._))
            .Throws(new Exception("Database error"));
        
        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrowsException_ThrowsException()
    {
        // Arrange
        var aboutItemId = Guid.NewGuid();
        var command = new UpdateAboutItemCommand(aboutItemId, new UpdateAboutItemDTO("Updated Title", "Updated Description", 3, false));
        var existingAboutItemResult = AboutItem.Create("Old Title", "Old Description", 2, true);
        var existingAboutItem = existingAboutItemResult.Value!;
        A.CallTo(() => _aboutItemCommandRepository.GetByIdAsync(aboutItemId, A<CancellationToken>._))
            .Returns(existingAboutItem);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Save changes error"));
        
        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Save changes error");
    }
}