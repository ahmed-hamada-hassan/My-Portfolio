using Application.Interfaces.DI;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ICertificateCommandRepository : IScopedMark
{
    Task<Certificate?> GetByIdAsync(Guid CertificateId, CancellationToken ct = default);
    Task AddAsync(Certificate certificate, CancellationToken ct = default);
    void Remove(Certificate certificate);
    Task<bool> CredentialURLExistsAsync(string CredentialURL, Guid? CertificateId, CancellationToken ct = default);
}
