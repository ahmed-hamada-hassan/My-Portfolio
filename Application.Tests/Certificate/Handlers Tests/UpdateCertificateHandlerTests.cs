using Application.Features.Certificate.Command;
using Application.Features.Certificate.DTOs;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Certificate.Handlers_Tests;

public class UpdateCertificateHandlerTests
{
    private readonly ICertificateCommandRepository _certificateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCertificateHandler> _logger;

    private readonly UpdateCertificateHandler _handler;

    public UpdateCertificateHandlerTests()
    {
        _certificateRepository = A.Fake<ICertificateCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<UpdateCertificateHandler>>();

        _handler = new UpdateCertificateHandler(
            _certificateRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var certificate = CreateCertificate();
        var request = new UpdateCertificateDTO(
            Name: "Updated Certificate Name",
            Issuer: "Updated Issuer",
            IssuerLogoURL: "https://example.com/updated-logo.png",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            CredentialURL: "https://example.com/updated-credential"
        );

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns(certificate);

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(request.CredentialURL!, certificateId, A<CancellationToken>._))
            .Returns(false);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        var command = new UpdateCertificateCommand(certificateId, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(request.CredentialURL!, certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        certificate.Name.Should().Be("Updated Certificate Name");
        certificate.Issuer.Should().Be("Updated Issuer");
        certificate.IssuerLogoURL.Should().Be("https://example.com/updated-logo.png");
        certificate.IssueDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)));
        certificate.CredentialURL.Should().Be("https://example.com/updated-credential");
    }

    [Fact]
    public async Task Handle_WhenCertificateNotFound_ReturnsNotFoundFailureResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var request = new UpdateCertificateDTO(
            Name: "Updated Name",
            Issuer: null,
            IssuerLogoURL: null,
            IssueDate: null,
            CredentialURL: null
        );

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns((Domain.Entities.Certificate?)null);

        var command = new UpdateCertificateCommand(certificateId, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CertificateErrors.NotFound);

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(A<string>._, A<Guid?>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenCredentialURLAlreadyExists_ReturnsDuplicateFailureResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var certificate = CreateCertificate();
        var request = new UpdateCertificateDTO(
            Name: null,
            Issuer: null,
            IssuerLogoURL: null,
            IssueDate: null,
            CredentialURL: "https://example.com/duplicate-credential"
        );

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns(certificate);

        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(request.CredentialURL!, certificateId, A<CancellationToken>._))
            .Returns(true);

        var command = new UpdateCertificateCommand(certificateId, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CertificateErrors.CredentialUrlAlreadyExists);

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _certificateRepository.CredentialURLExistsAsync(request.CredentialURL!, certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDomainUpdateFailsDueToFutureDate_ReturnsFailureResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var certificate = CreateCertificate();
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var request = new UpdateCertificateDTO(
            Name: null,
            Issuer: null,
            IssuerLogoURL: null,
            IssueDate: futureDate,
            CredentialURL: null
        );

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns(certificate);

        var command = new UpdateCertificateCommand(certificateId, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CertificateErrors.IssueDateInFuture);

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenDatabaseFailsOnSaveChanges_ThrowsException()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var certificate = CreateCertificate();
        var request = new UpdateCertificateDTO(
            Name: "Valid Name",
            Issuer: null,
            IssuerLogoURL: null,
            IssueDate: null,
            CredentialURL: null
        );

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns(certificate);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database save error"));

        var command = new UpdateCertificateCommand(certificateId, request);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database save error");

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    private static Domain.Entities.Certificate CreateCertificate()
    {
        var result = Domain.Entities.Certificate.Create(
            "Original Certificate Name",
            "Original Issuer",
            "https://example.com/original-logo.png",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            "https://example.com/original-credential",
            "https://example.com/original-cert.png",
            "cert-public-id"
        );

        return result.Value!;
    }
}
