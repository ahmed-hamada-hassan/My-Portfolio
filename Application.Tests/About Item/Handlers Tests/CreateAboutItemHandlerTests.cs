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

public class CreateAboutItemHandlerTests
{
    private readonly IAboutItemCommandRepository _aboutItemCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAboutItemHandler> _logger;

    private readonly CreateAboutItemHandler _handler;

    public CreateAboutItemHandlerTests()
    {
        _aboutItemCommandRepository = A.Fake<IAboutItemCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<CreateAboutItemHandler>>();

        _handler = new CreateAboutItemHandler(_aboutItemCommandRepository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessfulAboutItemIdResult()
    {
        // Arrange
        var command = new CreateAboutItemCommand(new CreateAboutItemDTO("Valid Title", "Valid Description", 2, true));
        A.CallTo(() => _aboutItemCommandRepository.TitleExistsAsync(command.CreateAboutItemRequest.Title, null, A<CancellationToken>._))
            .Returns(false);

        A.CallTo(() => _aboutItemCommandRepository.DisplayOrderExistsAsync(command.CreateAboutItemRequest.DisplayOrder, null, A<CancellationToken>._))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        A.CallTo(() => _aboutItemCommandRepository.AddAsync(A<AboutItem>.That.Matches(item =>
        item.Title == command.CreateAboutItemRequest.Title &&
        item.Description == command.CreateAboutItemRequest.Description &&
        item.DisplayOrder == command.CreateAboutItemRequest.DisplayOrder &&
        item.IsVisible == command.CreateAboutItemRequest.IsVisible),
        A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenTitleExists_ReturnsTitleAlreadyExistsError()
    {
        // Arrange
        var command = new CreateAboutItemCommand(new CreateAboutItemDTO("Existing Title", "Valid Description", 2, true));
        A.CallTo(() => _aboutItemCommandRepository.TitleExistsAsync(command.CreateAboutItemRequest.Title, null, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AboutItemErrors.TitleAlreadyExists);

        A.CallTo(() => _aboutItemCommandRepository.AddAsync(A<AboutItem>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDisplayOrderExists_ReturnsDisplayOrderAlreadyExistsError()
    {
        // Arrange
        var command = new CreateAboutItemCommand(new CreateAboutItemDTO("Valid Title", "Valid Description", 1, true));
        A.CallTo(() => _aboutItemCommandRepository.TitleExistsAsync(command.CreateAboutItemRequest.Title, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _aboutItemCommandRepository.DisplayOrderExistsAsync(command.CreateAboutItemRequest.DisplayOrder, null, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AboutItemErrors.DisplayOrderAlreadyExists);
        A.CallTo(() => _aboutItemCommandRepository.AddAsync(A<AboutItem>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ReturnsDomainValidationError()
    {
        // Arrange
        var command = new CreateAboutItemCommand(new CreateAboutItemDTO("", "Valid Description", 2, true));
        A.CallTo(() => _aboutItemCommandRepository.TitleExistsAsync(command.CreateAboutItemRequest.Title, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _aboutItemCommandRepository.DisplayOrderExistsAsync(command.CreateAboutItemRequest.DisplayOrder, null, A<CancellationToken>._))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(AboutItemErrors.InvalidTitle.Code);

        A.CallTo(() => _aboutItemCommandRepository.AddAsync(A<AboutItem>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_ThrowsException()
    {
        // Arrange
        var command = new CreateAboutItemCommand(new CreateAboutItemDTO("Valid Title", "Valid Description", 2, true));

        A.CallTo(() => _aboutItemCommandRepository.TitleExistsAsync(command.CreateAboutItemRequest.Title, null, A<CancellationToken>._))
            .Returns(false);

        A.CallTo(() => _aboutItemCommandRepository.DisplayOrderExistsAsync(command.CreateAboutItemRequest.DisplayOrder, null, A<CancellationToken>._))
            .Returns(false);

        A.CallTo(() => _aboutItemCommandRepository.AddAsync(A<AboutItem>._, A<CancellationToken>._))
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
        var command = new CreateAboutItemCommand(new CreateAboutItemDTO("Valid Title", "Valid Description", 2, true));
        A.CallTo(() => _aboutItemCommandRepository.TitleExistsAsync(command.CreateAboutItemRequest.Title, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _aboutItemCommandRepository.DisplayOrderExistsAsync(command.CreateAboutItemRequest.DisplayOrder, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
    }
}