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

public class UpdateContactLinkHandlerTests
{
    private readonly IContactLinkCommandRepository _contactLinkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateContactLinkHandler> _logger;
    private readonly UpdateContactLinkHandler _handler;

    public UpdateContactLinkHandlerTests()
    {
        _contactLinkRepository = A.Fake<IContactLinkCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<UpdateContactLinkHandler>>();

        _handler = new UpdateContactLinkHandler(
            _contactLinkRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var contactLinkId = Guid.NewGuid();
        var socialLink = CreateContactLink();
        var request = new UpdateContactLinkDTO(
            Name: "Updated Name",
            Contact: "https://updated.com",
            DisplayOrder: 3,
            IsVisible: false
        );
        var command = new UpdateContactLinkCommand(contactLinkId, request);

        A.CallTo(() => _contactLinkRepository.NameExistsAsync(request.Name!, contactLinkId, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.URLExistsAsync(request.Contact!, contactLinkId, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.DisplayOrderExistsAsync(request.DisplayOrder!.Value, contactLinkId, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.GetByIdAsync(contactLinkId, A<CancellationToken>._))
            .Returns(socialLink);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _contactLinkRepository.GetByIdAsync(contactLinkId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        socialLink.Name.Should().Be("Updated Name");
        socialLink.Contact.Should().Be("https://updated.com");
        socialLink.DisplayOrder.Should().Be(3);
        socialLink.IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenContactLinkNotFound_ReturnsNotFoundFailureResult()
    {
        // Arrange
        var contactLinkId = Guid.NewGuid();
        var request = new UpdateContactLinkDTO(
            Name: null,
            Contact: null,
            DisplayOrder: null,
            IsVisible: true
        );
        var command = new UpdateContactLinkCommand(contactLinkId, request);

        A.CallTo(() => _contactLinkRepository.GetByIdAsync(contactLinkId, A<CancellationToken>._))
            .Returns((ContactLink?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContactLinkErrors.SocialLinkNotFound);

        A.CallTo(() => _contactLinkRepository.GetByIdAsync(contactLinkId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var contactLinkId = Guid.NewGuid();
        var request = new UpdateContactLinkDTO(
            Name: "ExistingName",
            Contact: null,
            DisplayOrder: null,
            IsVisible: null
        );
        var command = new UpdateContactLinkCommand(contactLinkId, request);

        A.CallTo(() => _contactLinkRepository.NameExistsAsync("ExistingName", contactLinkId, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContactLinkErrors.NameAlreadyExists);

        A.CallTo(() => _contactLinkRepository.NameExistsAsync("ExistingName", contactLinkId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _contactLinkRepository.GetByIdAsync(A<Guid>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenURLAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var contactLinkId = Guid.NewGuid();
        var request = new UpdateContactLinkDTO(
            Name: null,
            Contact: "https://existing.com",
            DisplayOrder: null,
            IsVisible: null
        );
        var command = new UpdateContactLinkCommand(contactLinkId, request);

        A.CallTo(() => _contactLinkRepository.URLExistsAsync("https://existing.com", contactLinkId, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContactLinkErrors.URLAlreadyExists);

        A.CallTo(() => _contactLinkRepository.URLExistsAsync("https://existing.com", contactLinkId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _contactLinkRepository.GetByIdAsync(A<Guid>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDisplayOrderAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var contactLinkId = Guid.NewGuid();
        var request = new UpdateContactLinkDTO(
            Name: null,
            Contact: null,
            DisplayOrder: 2,
            IsVisible: null
        );
        var command = new UpdateContactLinkCommand(contactLinkId, request);

        A.CallTo(() => _contactLinkRepository.DisplayOrderExistsAsync(2, contactLinkId, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContactLinkErrors.DisplayOrderAlreadyExists);

        A.CallTo(() => _contactLinkRepository.DisplayOrderExistsAsync(2, contactLinkId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _contactLinkRepository.GetByIdAsync(A<Guid>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var contactLinkId = Guid.NewGuid();
        var socialLink = CreateContactLink();
        var request = new UpdateContactLinkDTO(
            Name: "New Name",
            Contact: null,
            DisplayOrder: null,
            IsVisible: null
        );
        var command = new UpdateContactLinkCommand(contactLinkId, request);

        A.CallTo(() => _contactLinkRepository.NameExistsAsync("New Name", contactLinkId, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _contactLinkRepository.GetByIdAsync(contactLinkId, A<CancellationToken>._))
            .Returns(socialLink);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _contactLinkRepository.GetByIdAsync(contactLinkId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    private static ContactLink CreateContactLink()
    {
        var result = ContactLink.Create(
            "Original Name",
            "https://original.com",
            1,
            true
        );
        return result.Value!;
    }
}
