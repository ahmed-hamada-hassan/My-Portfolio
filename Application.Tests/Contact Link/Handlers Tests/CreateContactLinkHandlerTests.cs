using Application.Features.Contact_Links.Command;
using Application.Features.Contact_Links.DTOs;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Contact_Link.Handlers_Tests;

public class CreateContactLinkHandlerTests
{
    private readonly IContactLinkCommandRepository _contactLinkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateContactLinkHandler> _logger;
    private readonly CreateContactLinkHandler _handler;

    public CreateContactLinkHandlerTests()
    {
        _contactLinkRepository = A.Fake<IContactLinkCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<CreateContactLinkHandler>>();

        _handler = new CreateContactLinkHandler(
            _contactLinkRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessfulIdResult()
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: "LinkedIn",
            Contact: "https://linkedin.com/in/test",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        A.CallTo(() => _contactLinkRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.URLExistsAsync(request.Contact, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.AddAsync(A<ContactLink>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        A.CallTo(() => _contactLinkRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _contactLinkRepository.URLExistsAsync(request.Contact, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _contactLinkRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _contactLinkRepository.AddAsync(A<ContactLink>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: "LinkedIn",
            Contact: "https://linkedin.com/in/test",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        A.CallTo(() => _contactLinkRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContactLinkErrors.NameAlreadyExists);

        A.CallTo(() => _contactLinkRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _contactLinkRepository.URLExistsAsync(A<string>._, A<Guid?>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _contactLinkRepository.AddAsync(A<ContactLink>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenURLAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: "LinkedIn",
            Contact: "https://linkedin.com/in/test",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        A.CallTo(() => _contactLinkRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.URLExistsAsync(request.Contact, null, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContactLinkErrors.URLAlreadyExists);

        A.CallTo(() => _contactLinkRepository.URLExistsAsync(request.Contact, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _contactLinkRepository.DisplayOrderExistsAsync(A<byte>._, A<Guid?>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _contactLinkRepository.AddAsync(A<ContactLink>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDisplayOrderAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: "LinkedIn",
            Contact: "https://linkedin.com/in/test",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        A.CallTo(() => _contactLinkRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.URLExistsAsync(request.Contact, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContactLinkErrors.DisplayOrderAlreadyExists);

        A.CallTo(() => _contactLinkRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _contactLinkRepository.AddAsync(A<ContactLink>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: "",
            Contact: "https://linkedin.com",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        A.CallTo(() => _contactLinkRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.URLExistsAsync(request.Contact, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, A<CancellationToken>._))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContactLinkErrors.InvalidName);

        A.CallTo(() => _contactLinkRepository.AddAsync(A<ContactLink>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: "LinkedIn",
            Contact: "https://linkedin.com",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        A.CallTo(() => _contactLinkRepository.NameExistsAsync(request.Name, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.URLExistsAsync(request.Contact, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.AddAsync(A<ContactLink>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _contactLinkRepository.AddAsync(A<ContactLink>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
