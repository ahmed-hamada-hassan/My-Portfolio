using Application.Features.Certificate.DTOs;
using Application.Interfaces.DI;

namespace Application.Interfaces.Repositories;

public interface ICertificateQueryRepository : IScopedMark
{
    Task<IReadOnlyList<GetCertificateDTO>> GetAllAsync(CancellationToken ct = default);
}
