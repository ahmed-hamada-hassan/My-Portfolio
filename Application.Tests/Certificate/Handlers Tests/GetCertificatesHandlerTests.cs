using Application.Features.Certificate.DTOs;
using Application.Features.Certificate.Query;
using Application.Interfaces.Repositories;
using FakeItEasy;
using FluentAssertions;

namespace Application.Tests.Certificate.Handlers_Tests;

public class GetCertificatesHandlerTests
{
    private readonly ICertificateQueryRepository _certificateQueryRepository;
    private readonly GetCertificatesHandler _handler;

    public GetCertificatesHandlerTests()
    {
        _certificateQueryRepository = A.Fake<ICertificateQueryRepository>();
        _handler = new GetCertificatesHandler(_certificateQueryRepository);
    }

    [Fact]
    public async Task Handle_WhenCertificatesExist_ReturnsSuccessfulResultWithCertificates()
    {
        // Arrange
        var query = new GetCertificatesQuery();
        var expectedCertificates = new List<GetCertificateDTO>
        {
            new(
                CertificateId: Guid.NewGuid(),
                CertificateTitle: "AWS Certified Developer",
                Issuer: "Amazon Web Services",
                IssuerLogoURL: "https://example.com/aws.png",
                IssueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)),
                CredentialURL: "https://example.com/cred1",
                ImageURL: "https://example.com/cert1.png"
            ),
            new(
                CertificateId: Guid.NewGuid(),
                CertificateTitle: "Azure Solutions Architect",
                Issuer: "Microsoft",
                IssuerLogoURL: "https://example.com/ms.png",
                IssueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
                CredentialURL: "https://example.com/cred2",
                ImageURL: "https://example.com/cert2.png"
            )
        };

        A.CallTo(() => _certificateQueryRepository.GetAllAsync(A<CancellationToken>._))
            .Returns(expectedCertificates);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedCertificates);

        A.CallTo(() => _certificateQueryRepository.GetAllAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenNoCertificatesExist_ReturnsSuccessfulResultWithEmptyList()
    {
        // Arrange
        var query = new GetCertificatesQuery();
        var emptyList = new List<GetCertificateDTO>();

        A.CallTo(() => _certificateQueryRepository.GetAllAsync(A<CancellationToken>._))
            .Returns(emptyList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        A.CallTo(() => _certificateQueryRepository.GetAllAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
