using Application.Features.Certificate.Command;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Certificate.Handlers_Tests;

public class UpdateCertificateImageHandlerTests
{
    private readonly ICertificateCommandRepository _certificateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<UpdateCertificateImageHandler> _logger;

    private readonly UpdateCertificateImageHandler _handler;

    public UpdateCertificateImageHandlerTests()
    {
        _certificateRepository = A.Fake<ICertificateCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _imageStorageService = A.Fake<IImageStorageService>();
        _logger = A.Fake<ILogger<UpdateCertificateImageHandler>>();

        _handler = new UpdateCertificateImageHandler(
            _certificateRepository,
            _unitOfWork,
            _imageStorageService,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessResultAndDeletesOldImage()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var certificate = CreateCertificate();
        var fakeImageFile = CreateFakeImageFile();
        var oldImageUrl = certificate.ImageURL;
        var oldImagePublicId = certificate.ImagePublicId;

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns(certificate);

        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/new-image.jpg", "new-public-id")));

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        A.CallTo(() => _imageStorageService.DeleteImageAsync(oldImageUrl, oldImagePublicId))
            .Returns(Result<bool>.Success(true));

        var command = new UpdateCertificateImageCommand(certificateId, fakeImageFile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(oldImageUrl, oldImagePublicId))
            .MustHaveHappenedOnceExactly();

        certificate.ImageURL.Should().Be("https://example.com/new-image.jpg");
        certificate.ImagePublicId.Should().Be("new-public-id");
    }

    [Fact]
    public async Task Handle_WhenCertificateNotFound_ReturnsNotFoundFailureResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var fakeImageFile = CreateFakeImageFile();

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns((Domain.Entities.Certificate?)null);

        var command = new UpdateCertificateImageCommand(certificateId, fakeImageFile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CertificateErrors.NotFound);

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(A<IFormFile>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenImageUploadFails_ReturnsFailureResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var certificate = CreateCertificate();
        var fakeImageFile = CreateFakeImageFile();
        var uploadError = new Error("UploadFailed", "Failed to upload image.");

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns(certificate);

        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Failure(uploadError));

        var command = new UpdateCertificateImageCommand(certificateId, fakeImageFile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(uploadError);

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_RollsBackNewImageAndThrowsException()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var certificate = CreateCertificate();
        var fakeImageFile = CreateFakeImageFile();
        var newImageUrl = "https://example.com/new-image.jpg";
        var newImagePublicId = "new-public-id";

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns(certificate);

        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success((newImageUrl, newImagePublicId)));

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        A.CallTo(() => _imageStorageService.DeleteImageAsync(newImageUrl, newImagePublicId))
            .Returns(Result<bool>.Success(true));

        var command = new UpdateCertificateImageCommand(certificateId, fakeImageFile);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(newImageUrl, newImagePublicId))
            .MustHaveHappenedOnceExactly();
    }

    private static Domain.Entities.Certificate CreateCertificate()
    {
        var result = Domain.Entities.Certificate.Create(
            "Test Certificate Name",
            "Test Certificate Issuer",
            "https://example.com/logo.png",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            "https://example.com/credential",
            "https://example.com/original-image.png",
            "original-public-id"
        );

        return result.Value!;
    }

    private static IFormFile CreateFakeImageFile()
    {
        var file = A.Fake<IFormFile>();
        A.CallTo(() => file.FileName).Returns("test-image.jpg");
        A.CallTo(() => file.Length).Returns(2048);
        A.CallTo(() => file.ContentType).Returns("image/jpeg");
        return file;
    }
}
