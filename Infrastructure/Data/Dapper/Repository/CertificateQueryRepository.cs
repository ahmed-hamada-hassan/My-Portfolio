using Application.Features.Certificate.DTOs;
using Application.Interfaces.Repositories;
using Dapper;

namespace Infrastructure.Data.Dapper.Repository;

internal sealed class CertificateQueryRepository : ICertificateQueryRepository
{
    private readonly DapperContext _context;

    public CertificateQueryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GetCertificateDTO>> GetAllAsync(CancellationToken ct = default)
    {
        var query = """
            SELECT
                Id As CertificateId,
            	Name AS CertificateTitle,
            	Issuer,
            	IssuerLogoURL,
            	IssueDate,
            	CredentialURL,
            	ImageURL
            FROM
            	Certificates
            ORDER BY IssueDate DESC;
            """;

        using var connection = _context.CreateConnection();
        var command = new CommandDefinition(query, cancellationToken: ct);
        var certificates = await connection.QueryAsync<GetCertificateDTO>(command);

        return certificates.ToList();
    }
}
