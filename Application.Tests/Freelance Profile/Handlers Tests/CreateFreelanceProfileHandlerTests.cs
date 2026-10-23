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

public class CreateFreelanceProfileHandlerTests
{
    private readonly IFreelanceProfileCommandRepository _freelanceProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFreelanceProfileHandler> _logger;
    private readonly CreateFreelanceProfileHandler _handler;

    public CreateFreelanceProfileHandlerTests()
    {
        _freelanceProfileRepository = A.Fake<IFreelanceProfileCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<CreateFreelanceProfileHandler>>();

        _handler = new CreateFreelanceProfileHandler(
            _freelanceProfileRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessfulIdResult()
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: "https://www.upwork.com/freelancers/~01test",
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.ProfileURLExistsAsync(request.ProfileURL, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.IconExistsAsync(request.Icon, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.AddAsync(A<FreelanceProfile>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _freelanceProfileRepository.ProfileURLExistsAsync(request.ProfileURL, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _freelanceProfileRepository.IconExistsAsync(request.Icon, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _freelanceProfileRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _freelanceProfileRepository.AddAsync(A<FreelanceProfile>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenPlatformNameAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: "https://www.upwork.com/freelancers/~01test",
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName, null, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.PlatformNameAlreadyExists);

        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _freelanceProfileRepository.ProfileURLExistsAsync(A<string>._, A<Guid?>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _freelanceProfileRepository.AddAsync(A<FreelanceProfile>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenProfileUrlAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: "https://www.upwork.com/freelancers/~01test",
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.ProfileURLExistsAsync(request.ProfileURL, null, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.ProfileUrlAlreadyExists);

        A.CallTo(() => _freelanceProfileRepository.ProfileURLExistsAsync(request.ProfileURL, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _freelanceProfileRepository.IconExistsAsync(A<string>._, A<Guid?>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _freelanceProfileRepository.AddAsync(A<FreelanceProfile>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenIconAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: "https://www.upwork.com/freelancers/~01test",
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.ProfileURLExistsAsync(request.ProfileURL, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.IconExistsAsync(request.Icon, null, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.IconAlreadyExists);

        A.CallTo(() => _freelanceProfileRepository.IconExistsAsync(request.Icon, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _freelanceProfileRepository.DisplayOrderExistsAsync(A<byte>._, A<Guid?>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _freelanceProfileRepository.AddAsync(A<FreelanceProfile>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDisplayOrderAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: "https://www.upwork.com/freelancers/~01test",
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.ProfileURLExistsAsync(request.ProfileURL, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.IconExistsAsync(request.Icon, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, A<CancellationToken>._))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.DisplayOrderAlreadyExists);

        A.CallTo(() => _freelanceProfileRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _freelanceProfileRepository.AddAsync(A<FreelanceProfile>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "",
            ProfileURL: "https://www.upwork.com/freelancers/~01test",
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.ProfileURLExistsAsync(request.ProfileURL, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.IconExistsAsync(request.Icon, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, A<CancellationToken>._))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FreelanceProfileErrors.InvalidPlatformName);

        A.CallTo(() => _freelanceProfileRepository.AddAsync(A<FreelanceProfile>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: "https://www.upwork.com/freelancers/~01test",
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        A.CallTo(() => _freelanceProfileRepository.PlatformNameExistsAsync(request.PlatformName, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.ProfileURLExistsAsync(request.ProfileURL, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.IconExistsAsync(request.Icon, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _freelanceProfileRepository.AddAsync(A<FreelanceProfile>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _freelanceProfileRepository.AddAsync(A<FreelanceProfile>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
