using Application.Features.Freelance_Profile.Command;
using Application.Features.Freelance_Profile.DTOs;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Freelance_Profile.Handlers_Tests;

public class UpdateFreelanceProfileHandlerTests
{
    private readonly IFreelanceProfileCommandRepository _freelanceProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFreelanceProfileHandler> _logger;
    private readonly UpdateFreelanceProfileHandler _handler;

    public UpdateFreelanceProfileHandlerTests()
    {
        _freelanceProfileRepository = A.Fake<IFreelanceProfileCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<UpdateFreelanceProfileHandler>>();

        _handler = new UpdateFreelanceProfileHandler(
            _freelanceProfileRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var profile = FreelanceProfile.Create(
            "Upwork",
            "https://upwork.com/freelancers/~test",
            "https://cdn.example.com/upwork.svg",
            1,
            true
        ).Value!;

        var request = new UpdateFreelanceProfileDTO(
            PlatformName: "Fiverr",
            ProfileURL: "https://fiverr.com/test",
            Icon: "https://cdn.example.com/fiverr.svg",
            DisplayOrder: 2,
            IsVisible: false
        );
        var command = new UpdateFreelanceProfileCommand(profile.Id, request);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profile.Id, A<CancellationToken>._))
            .Returns(profile);
        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName!, profile.Id, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.ProfileURLExistsAsync(request.ProfileURL!, profile.Id, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.IconExistsAsync(request.Icon!, profile.Id, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.DisplayOrderExistsAsync(request.DisplayOrder!.Value, profile.Id, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        profile.PlatformName.Should().Be("Fiverr");
        profile.ProfileURL.Should().Be("https://fiverr.com/test");
        profile.Icon.Should().Be("https://cdn.example.com/fiverr.svg");
        profile.DisplayOrder.Should().Be(2);
        profile.IsVisible.Should().BeFalse();

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profile.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsFailureResult()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var request = new UpdateFreelanceProfileDTO("Fiverr", null, null, null, null);
        var command = new UpdateFreelanceProfileCommand(profileId, request);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profileId, A<CancellationToken>._))
            .Returns((FreelanceProfile?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.FreelanceProfileNotFound);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profileId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenPlatformNameAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var profile = FreelanceProfile.Create(
            "Upwork",
            "https://upwork.com/freelancers/~test",
            "https://cdn.example.com/upwork.svg",
            1,
            true
        ).Value!;

        var request = new UpdateFreelanceProfileDTO("ExistingPlatform", null, null, null, null);
        var command = new UpdateFreelanceProfileCommand(profile.Id, request);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profile.Id, A<CancellationToken>._))
            .Returns(profile);
        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName!, profile.Id, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.PlatformNameAlreadyExists);

        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName!, profile.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenProfileUrlAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var profile = FreelanceProfile.Create(
            "Upwork",
            "https://upwork.com/freelancers/~test",
            "https://cdn.example.com/upwork.svg",
            1,
            true
        ).Value!;

        var request = new UpdateFreelanceProfileDTO(null, "https://upwork.com/existing", null, null, null);
        var command = new UpdateFreelanceProfileCommand(profile.Id, request);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profile.Id, A<CancellationToken>._))
            .Returns(profile);
        A.CallTo(() => _freelanceProfileRepository.ProfileURLExistsAsync(request.ProfileURL!, profile.Id, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.ProfileUrlAlreadyExists);

        A.CallTo(() => _freelanceProfileRepository.ProfileURLExistsAsync(request.ProfileURL!, profile.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenIconAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var profile = FreelanceProfile.Create(
            "Upwork",
            "https://upwork.com/freelancers/~test",
            "https://cdn.example.com/upwork.svg",
            1,
            true
        ).Value!;

        var request = new UpdateFreelanceProfileDTO(null, null, "https://cdn.example.com/existing.svg", null, null);
        var command = new UpdateFreelanceProfileCommand(profile.Id, request);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profile.Id, A<CancellationToken>._))
            .Returns(profile);
        A.CallTo(() => _freelanceProfileRepository.IconExistsAsync(request.Icon!, profile.Id, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.IconAlreadyExists);

        A.CallTo(() => _freelanceProfileRepository.IconExistsAsync(request.Icon!, profile.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDisplayOrderAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var profile = FreelanceProfile.Create(
            "Upwork",
            "https://upwork.com/freelancers/~test",
            "https://cdn.example.com/upwork.svg",
            1,
            true
        ).Value!;

        var request = new UpdateFreelanceProfileDTO(null, null, null, 2, null);
        var command = new UpdateFreelanceProfileCommand(profile.Id, request);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profile.Id, A<CancellationToken>._))
            .Returns(profile);
        A.CallTo(() => _freelanceProfileRepository.DisplayOrderExistsAsync(request.DisplayOrder!.Value, profile.Id, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.DisplayOrderAlreadyExists);

        A.CallTo(() => _freelanceProfileRepository.DisplayOrderExistsAsync(request.DisplayOrder!.Value, profile.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var profile = FreelanceProfile.Create(
            "Upwork",
            "https://upwork.com/freelancers/~test",
            "https://cdn.example.com/upwork.svg",
            1,
            true
        ).Value!;

        var request = new UpdateFreelanceProfileDTO("   ", null, null, null, null);
        var command = new UpdateFreelanceProfileCommand(profile.Id, request);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profile.Id, A<CancellationToken>._))
            .Returns(profile);
        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName!, profile.Id, A<CancellationToken>._))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.InvalidPlatformName);

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

        var request = new UpdateFreelanceProfileDTO("Freelancer", null, null, null, null);
        var command = new UpdateFreelanceProfileCommand(profile.Id, request);

        A.CallTo(() => _freelanceProfileRepository.GetByIdAsync(profile.Id, A<CancellationToken>._))
            .Returns(profile);
        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName!, profile.Id, A<CancellationToken>._))
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
