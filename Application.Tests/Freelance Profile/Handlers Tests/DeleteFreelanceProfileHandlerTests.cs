using Application.Features.Freelance_Profile.Command;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Freelance_Profile.Handlers_Tests;

public class DeleteFreelanceProfileHandlerTests
{
    private readonly IFreelanceProfileCommandRepository _freelanceProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFreelanceProfileHandler> _logger;
    private readonly DeleteFreelanceProfileHandler _handler;

    public DeleteFreelanceProfileHandlerTests()
    {
        _freelanceProfileRepository = A.Fake<IFreelanceProfileCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<DeleteFreelanceProfileHandler>>();

        _handler = new DeleteFreelanceProfileHandler(
            _freelanceProfileRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WhenProfileExists_DeletesProfileAndReturnsSuccess()
    {
        // Arrange
        var profile = FreelanceProfile.Create(
            "Upwork",
            "https://upwork.com/freelancers/~test",
            "https://cdn.example.com/upwork.svg",
            1,
            true
        ).Value!;
        var command = new DeleteFreelanceProfileCommand(profile.Id);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profile.Id, A<CancellationToken>._))
            .Returns(profile);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profile.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _freelanceProfileRepository.Remove(profile))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsFailureResult()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var command = new DeleteFreelanceProfileCommand(profileId);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profileId, A<CancellationToken>._))
            .Returns((FreelanceProfile?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.FreelanceProfileNotFound);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profileId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _freelanceProfileRepository.Remove(A<FreelanceProfile>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var profile = FreelanceProfile.Create(
            "Upwork",
            "https://upwork.com/freelancers/~test",
            "https://cdn.example.com/upwork.svg",
            1,
            true
        ).Value!;
        var command = new DeleteFreelanceProfileCommand(profile.Id);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profile.Id, A<CancellationToken>._))
            .Returns(profile);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _freelanceProfileRepository.Remove(profile))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
