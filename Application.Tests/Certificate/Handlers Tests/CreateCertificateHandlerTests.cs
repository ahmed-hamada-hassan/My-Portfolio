using Application.Features.Certificate.Command;
using Application.Features.Certificate.DTOs;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit.Sdk;

namespace Application.Tests.Certificate.Handlers_Tests;

public class CreateCertificateHandlerTests
{
    private readonly ICertificateCommandRepository _certificateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<CreateCertificateHandler> _logger;

    private readonly CreateCertificateHandler _handler;

    public CreateCertificateHandlerTests()
    {
        _certificateRepository = A.Fake<ICertificateCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _imageStorageService = A.Fake<IImageStorageService>();
        _logger = A.Fake<ILogger<CreateCertificateHandler>>();

        _handler = new CreateCertificateHandler(
            _certificateRepository,
            _unitOfWork,
            _imageStorageService,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessfulCertificateIdResult()
    {
        // Arrange
        var fakeImageFile = A.Fake<IFormFile>();
        A.CallTo(() => fakeImageFile.FileName).Returns("test-image.jpg");
        A.CallTo(() => fakeImageFile.Length).Returns(1024); // 1 KB
        A.CallTo(() => fakeImageFile.ContentType).Returns("image/jpeg");

        var createCertificateRequest = new CreateCertificateDTO(
            "Test Certificate Name",
            "Test Certificate Issuer",
            "Test Certificate IssuerLogoURL",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Test Certificate CredentialURL",
            fakeImageFile
        );

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .Returns(false);

        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/test-image.jpg", "test-image-public-id")));

        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        // Act 
        var result = await _handler.Handle(new CreateCertificateCommand(createCertificateRequest), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithInvalidCredentialURL_ReturnsSuccessfulCertificateIdResult(string? invalidCredentialURL)
    {
        // Arrange
        var fakeImageFile = A.Fake<IFormFile>();

        A.CallTo(() => fakeImageFile.FileName).Returns("test-image.jpg");
        A.CallTo(() => fakeImageFile.Length).Returns(1024); // 1 KB
        A.CallTo(() => fakeImageFile.ContentType).Returns("image/jpeg");

        var createCertificateRequest = new CreateCertificateDTO(
            "Test Certificate Name",
            "Test Certificate Issuer",
            "Test Certificate IssuerLogoURL",
            DateOnly.FromDateTime(DateTime.UtcNow),
            invalidCredentialURL,
            fakeImageFile
        );

        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/test-image.jpg", "test-image-public-id")));

        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        // Act 
        var result = await _handler.Handle(new CreateCertificateCommand(createCertificateRequest), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(A<string>._, A<Guid?>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WithDuplicateCredentialURL_ReturnsFailureResult()
    {
        // Arrange
        var fakeImageFile = A.Fake<IFormFile>();

        A.CallTo(() => fakeImageFile.FileName).Returns("test-image.jpg");
        A.CallTo(() => fakeImageFile.Length).Returns(1024); // 1 KB
        A.CallTo(() => fakeImageFile.ContentType).Returns("image/jpeg");

        var createCertificateRequest = new CreateCertificateDTO(
            "Test Certificate Name",
            "Test Certificate Issuer",
            "Test Certificate IssuerLogoURL",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "https://example.com/duplicate-credential-url",
            fakeImageFile
        );

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .Returns(true);

        // Act 
        var result = await _handler.Handle(new CreateCertificateCommand(createCertificateRequest), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(CertificateErrors.CredentialUrlAlreadyExists);

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(A<IFormFile>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WithImageUploadFailure_ReturnsFailureResult()
    {
        // Arrange
        var fakeImageFile = A.Fake<IFormFile>();

        A.CallTo(() => fakeImageFile.FileName).Returns("test-image.jpg");
        A.CallTo(() => fakeImageFile.Length).Returns(1024); // 1 KB
        A.CallTo(() => fakeImageFile.ContentType).Returns("image/jpeg");

        var createCertificateRequest = new CreateCertificateDTO(
            "Test Certificate Name",
            "Test Certificate Issuer",
            "Test Certificate IssuerLogoURL",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "https://example.com/test-credential-url",
            fakeImageFile
        );

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .Returns(false);

        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Failure(new Error("Image upload failed", "An error occurred while uploading the image.")));

        // Act 
        var result = await _handler.Handle(new CreateCertificateCommand(createCertificateRequest), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(new Error("Image upload failed", "An error occurred while uploading the image."));

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDomainCreateFaild_ReturnsFailureResult()
    {
        // Arrange
        var fakeImageFile = A.Fake<IFormFile>();
        A.CallTo(() => fakeImageFile.FileName).Returns("test-image.jpg");
        A.CallTo(() => fakeImageFile.Length).Returns(1024); // 1 KB
        A.CallTo(() => fakeImageFile.ContentType).Returns("image/jpeg");
        var createCertificateRequest = new CreateCertificateDTO(
            "",
            "Test Certificate Issuer",
            "Test Certificate IssuerLogoURL",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "https://example.com/test-credential-url",
            fakeImageFile
        );
        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/test-image.jpg", "test-image-public-id")));
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .Returns(Result<bool>.Success(true));

        // Act 
        var result = await _handler.Handle(new CreateCertificateCommand(createCertificateRequest), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CertificateErrors.InvalidName);

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenRepositoryAddFaild_ThrowsException()
    {
        // Arrange
        var fakeImageFile = A.Fake<IFormFile>();

        A.CallTo(() => fakeImageFile.FileName).Returns("test-image.jpg");
        A.CallTo(() => fakeImageFile.Length).Returns(1024); // 1 KB
        A.CallTo(() => fakeImageFile.ContentType).Returns("image/jpeg");
        
        var createCertificateRequest = new CreateCertificateDTO(
            "Test Certificate Name",
            "Test Certificate Issuer",
            "Test Certificate IssuerLogoURL",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "https://example.com/test-credential-url",
            fakeImageFile
        );

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/test-image.jpg", "test-image-public-id")));
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .Throws(new Exception("Database error"));
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .Returns(Result<bool>.Success(true));

        // Act 
        Func<Task> act = async () => await _handler.Handle(new CreateCertificateCommand(createCertificateRequest), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenRepositoryAddFaildAndImageDeleteFaild_ThrowsExceptionAndLogsFaildToDeleteOrphanedImage()
    {
        // Arrange
        var fakeImageFile = A.Fake<IFormFile>();
        A.CallTo(() => fakeImageFile.FileName).Returns("test-image.jpg");
        A.CallTo(() => fakeImageFile.Length).Returns(1024); // 1 KB
        A.CallTo(() => fakeImageFile.ContentType).Returns("image/jpeg");

        var createCertificateRequest = new CreateCertificateDTO(
            "Test Certificate Name",
            "Test Certificate Issuer",
            "Test Certificate IssuerLogoURL",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "https://example.com/test-credential-url",
            fakeImageFile
        );
        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/test-image.jpg", "test-image-public-id")));
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .Throws(new Exception("Database error"));
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .Throws(new Exception("Image deletion error"));
        
        // Act 
        Func<Task> act = async () => await _handler.Handle(new CreateCertificateCommand(createCertificateRequest), CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(_logger).Where(call => 
        call.Method.Name == "Log" &&
        call.GetArgument<LogLevel>(0) == LogLevel.Error &&
        call.Arguments[2] != null &&
        call.Arguments[2]!.ToString()!.Contains("Failed to delete orphaned image"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenRepositoryAddFaildAndImageDeleteSuccess_ThrowsException()
    {
        // Arrange
        var fakeImageFile = A.Fake<IFormFile>();
        A.CallTo(() => fakeImageFile.FileName).Returns("test-image.jpg");
        A.CallTo(() => fakeImageFile.Length).Returns(1024); // 1 KB
        A.CallTo(() => fakeImageFile.ContentType).Returns("image/jpeg");

        var createCertificateRequest = new CreateCertificateDTO(
            "Test Certificate Name",
            "Test Certificate Issuer",
            "Test Certificate IssuerLogoURL",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "https://example.com/test-credential-url",
            fakeImageFile
        );

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .Returns(false);
        
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/test-image.jpg", "test-image-public-id")));
        
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .Throws(new Exception("Database error"));

        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .Returns(Result<bool>.Success(true));

        // Act 
        Func<Task> act = async () => await _handler.Handle(new CreateCertificateCommand(createCertificateRequest), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenSavingChangesInDatabaseFaildAndDeleteImageSuccess_ThrowException()
    {
        // Arrange
        var fakeImageFile = A.Fake<IFormFile>();

        A.CallTo(() => fakeImageFile.FileName).Returns("test-image.jpg");
        A.CallTo(() => fakeImageFile.Length).Returns(1024); // 1 KB
        A.CallTo(() => fakeImageFile.ContentType).Returns("image/jpeg");

        var createCertificateRequest = new CreateCertificateDTO(
            "Test Certificate Name",
            "Test Certificate Issuer",
            "Test Certificate IssuerLogoURL",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "https://example.com/test-credential-url",
            fakeImageFile
        );

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .Returns(false);

        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/test-image.jpg", "test-image-public-id")));

        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .Returns(Result<bool>.Success(true));
        
        // Act 
        Func<Task> act = async () => await _handler.Handle(new CreateCertificateCommand(createCertificateRequest), CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenSavingChangesInDatabaseFaildAndDeleteImageFaild_ThrowsExceptionAndLogsFaildToDeleteOrphanedImage()
    {
        // Arrange
        var fakeImageFile = A.Fake<IFormFile>();
        A.CallTo(() => fakeImageFile.FileName).Returns("test-image.jpg");
        A.CallTo(() => fakeImageFile.Length).Returns(1024); // 1 KB
        A.CallTo(() => fakeImageFile.ContentType).Returns("image/jpeg");
        var createCertificateRequest = new CreateCertificateDTO(
            "Test Certificate Name",
            "Test Certificate Issuer",
            "Test Certificate IssuerLogoURL",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "https://example.com/test-credential-url",
            fakeImageFile
        );
        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .Returns(Result<(string URL, string PublicId)>.Success(("https://example.com/test-image.jpg", "test-image-public-id")));
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .Throws(new Exception("Image deletion error"));

        // Act 
        Func<Task> act = async () => await _handler.Handle(new CreateCertificateCommand(createCertificateRequest), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");
        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(createCertificateRequest.CredentialURL!, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.UploadImageAsync(fakeImageFile, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.AddAsync(A<Domain.Entities.Certificate>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _imageStorageService.DeleteImageAsync(A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(_logger).Where(call =>
        call.Method.Name == "Log" &&
        call.GetArgument<LogLevel>(0) == LogLevel.Error &&
        call.Arguments[2] != null &&
        call.Arguments[2]!.ToString()!.Contains("Failed to delete orphaned image"))
            .MustHaveHappenedOnceExactly();
    }
}