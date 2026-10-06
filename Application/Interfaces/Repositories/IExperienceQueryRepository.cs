using Application.Features.Experience.DTOs;
using Application.Interfaces.DI;

namespace Application.Interfaces.Repositories;

public interface IExperienceQueryRepository : IScopedMark
{
    Task<IReadOnlyList<GetExperienceListItemDTO>> GetAllAsync(CancellationToken ct = default);
    Task<GetExperienceDetailsDTO?> GetByIdAsync(Guid experienceId, CancellationToken ct = default);
}
