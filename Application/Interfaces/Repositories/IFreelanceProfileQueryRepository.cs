using Application.Features.Freelance_Profile.DTOs;
using Application.Interfaces.DI;

namespace Application.Interfaces.Repositories;

public interface IFreelanceProfileQueryRepository : IScopedMark
{
    Task<IReadOnlyList<GetFreelanceProfileDTO>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GetDetailsFreelanceProfileDTO>> GetDetailsAsync(
        CancellationToken cancellationToken = default);

    Task<GetDetailsFreelanceProfileDTO?> GetDetailsByIdAsync(
        Guid FreelanceProfileId,
        CancellationToken cancellationToken = default);
}
