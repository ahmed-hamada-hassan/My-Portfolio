using Application.Features.Contact_Links.Command;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Contact_Link.Handlers_Tests;

public class DeleteContactLinkHandlerTests
{
    private readonly IContactLinkCommandRepository _contactLinkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteContactLinkHandler> _logger;
    private readonly DeleteContactLinkHandler _handler;

    public DeleteContactLinkHandlerTests()
    {
        _contactLinkRepository = A.Fake<IContactLinkCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<DeleteContactLinkHandler>>();

        _handler = new DeleteContactLinkHandler(
            _contactLinkRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsSuccessResult()
    {
        // Arrange
        var contactLinkId = Guid.NewGuid();
        var socialLink = CreateContactLink();

        A.CallTo(() => _contactLinkRepository.GetByIdAsync(contactLinkId, A<CancellationToken>._))
            .Returns(socialLink);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        var command = new DeleteContactLinkCommand(contactLinkId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _contactLinkRepository.GetByIdAsync(contactLinkId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _contactLinkRepository.Remove(socialLink))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenContactLinkNotFound_ReturnsNotFoundFailureResult()
    {
        // Arrange
        var contactLinkId = Guid.NewGuid();

        A.CallTo(() => _contactLinkRepository.GetByIdAsync(contactLinkId, A<CancellationToken>._))
            .Returns((ContactLink?)null);

        var command = new DeleteContactLinkCommand(contactLinkId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContactLinkErrors.SocialLinkNotFound);

        A.CallTo(() => _contactLinkRepository.GetByIdAsync(contactLinkId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _contactLinkRepository.Remove(A<ContactLink>._))
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

        A.CallTo(() => _contactLinkRepository.GetByIdAsync(contactLinkId, A<CancellationToken>._))
            .Returns(socialLink);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        var command = new DeleteContactLinkCommand(contactLinkId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _contactLinkRepository.GetByIdAsync(contactLinkId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _contactLinkRepository.Remove(socialLink))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    private static ContactLink CreateContactLink()
    {
        var result = ContactLink.Create(
            "Test Link",
            "https://test.com",
            1,
            true
        );
        return result.Value!;
    }
}
