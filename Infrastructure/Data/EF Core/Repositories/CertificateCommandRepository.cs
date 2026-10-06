using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.EF_Core.Repositories;

internal sealed class CertificateCommandRepository : ICertificateCommandRepository
{
    private readonly AppDbContext _dbContext;

    public CertificateCommandRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Certificate certificate, CancellationToken ct = default)
    {
        await _dbContext.Certificates.AddAsync(certificate, ct);
    }

    public Task<Certificate?> GetByIdAsync(Guid CertificateId, CancellationToken ct = default)
    {
        return _dbContext.Certificates.FirstOrDefaultAsync(c => c.Id == CertificateId, ct);
    }

    public void Remove(Certificate certificate)
    {
        _dbContext.Certificates.Remove(certificate);
    }

    public Task<bool> CredentialURLExistsAsync(string CredentialURL, Guid? CertificateId, CancellationToken ct = default)
    {
        return _dbContext.Certificates
            .AnyAsync(c => c.CredentialURL == CredentialURL && (!CertificateId.HasValue || c.Id != CertificateId.Value), ct);
    }
}
