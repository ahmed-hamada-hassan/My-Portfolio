using Application.Features.Certificate.Command;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Certificate.Handlers_Tests;

public class DeleteCertificateHandlerTests
{
    private readonly ICertificateCommandRepository _certificateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<DeleteCertificateHandler> _logger;

    private readonly DeleteCertificateHandler _handler;

    public DeleteCertificateHandlerTests()
    {
        _certificateRepository = A.Fake<ICertificateCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _imageStorageService = A.Fake<IImageStorageService>();
        _logger = A.Fake<ILogger<DeleteCertificateHandler>>();

        _handler = new DeleteCertificateHandler(
            _certificateRepository,
            _unitOfWork,
            _imageStorageService,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidCertificateId_ReturnsSuccessResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var certificate = CreateCertificate();

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns(certificate);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(0);

        A.CallTo(() => _imageStorageService.DeleteImageAsync(certificate.ImageURL, certificate.ImagePublicId))
            .Returns(Result<bool>.Success(true));

        var command = new DeleteCertificateCommand(certificateId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.Remove(certificate))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(certificate.ImageURL, certificate.ImagePublicId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenCertificateNotFound_ReturnsNotFoundFailureResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns((Domain.Entities.Certificate?)null);

        var command = new DeleteCertificateCommand(certificateId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CertificateErrors.NotFound);

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.Remove(A<Domain.Entities.Certificate>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenImageDeletionFails_ReturnsFailureResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var certificate = CreateCertificate();
        var imageError = new Error("ImageDeleteFailed", "Failed to delete image.");

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns(certificate);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(0);

        A.CallTo(() => _imageStorageService.DeleteImageAsync(certificate.ImageURL, certificate.ImagePublicId))
            .Returns(Result<bool>.Failure(imageError));

        var command = new DeleteCertificateCommand(certificateId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(imageError);

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.Remove(certificate))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(certificate.ImageURL, certificate.ImagePublicId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var certificate = CreateCertificate();

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns(certificate);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        var command = new DeleteCertificateCommand(certificateId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.Remove(certificate))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    private static Domain.Entities.Certificate CreateCertificate()
    {
        var result = Domain.Entities.Certificate.Create(
            "Test Certificate Name",
            "Test Certificate Issuer",
            "https://example.com/logo.png",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "https://example.com/credential",
            "https://example.com/cert.png",
            "cert-public-id"
        );

        return result.Value!;
    }
}
