using Application.Features.Certificate.Command;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Certificate.Handlers_Tests;

public class ClearCredentialURLHandlerTests
{
    private readonly ICertificateCommandRepository _certificateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClearCredentialURLHandler> _logger;

    private readonly ClearCredentialURLHandler _handler;

    public ClearCredentialURLHandlerTests()
    {
        _certificateRepository = A.Fake<ICertificateCommandRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _logger = A.Fake<ILogger<ClearCredentialURLHandler>>();

        _handler = new ClearCredentialURLHandler(
            _certificateRepository,
            _unitOfWork,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessfulResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();

        var command = new ClearCredentialURLCommand(certificateId);

        var certificateResult = Domain.Entities.Certificate.Create(
                "Test Certificate Name",
                "Test Certificate Issuer",
                "Test Certificate IssuerLogoURL",
                DateOnly.FromDateTime(DateTime.Today),
                "Test Certificate CredentialURL",
                "Test Certificate ImageURL",
                "Test Certificate Image Public Id"
        );

        var certificate = certificateResult.Value!;

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns(certificate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        certificate.Name.Should().Be("Test Certificate Name");
        certificate.Issuer.Should().Be("Test Certificate Issuer");
        certificate.IssuerLogoURL.Should().Be("Test Certificate IssuerLogoURL");
        certificate.IssueDate.Should().Be(DateOnly.FromDateTime(DateTime.Today));
        certificate.CredentialURL.Should().BeNull();
        certificate.ImageURL.Should().Be("Test Certificate ImageURL");
        certificate.ImagePublicId.Should().Be("Test Certificate Image Public Id");
    }

    [Fact]
    public async Task Handle_WithNonExistentCertificate_ReturnsFailureResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();

        var command = new ClearCredentialURLCommand(certificateId);

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns((Domain.Entities.Certificate?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(CertificateErrors.NotFound);

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WithExceptionDuringSaveChanges_ReturnsFailureResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();

        var command = new ClearCredentialURLCommand(certificateId);

        var certificateResult = Domain.Entities.Certificate.Create(
                "Test Certificate Name",
                "Test Certificate Issuer",
                "Test Certificate IssuerLogoURL",
                DateOnly.FromDateTime(DateTime.Today),
                "Test Certificate CredentialURL",
                "Test Certificate ImageURL",
                "Test Certificate Image Public Id"
        );
        var certificate = certificateResult.Value!;

        A.CallTo(() => _certificateRepository.GetByIdAsync(certificateId, A<CancellationToken>._))
            .Returns(certificate);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
    }
}