using Application.Interfaces.DI;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IFreelanceProfileCommandRepository : IScopedMark
{
    Task<FreelanceProfile?> GetByIdAsync(Guid FreelanceProfileId, CancellationToken cancellationToken = default);
    Task<bool> PlatformNameExistsAsync(string PlatformName, Guid? FreelanceProfileId, CancellationToken cancellationToken = default);
    Task<bool> ProfileURLExistsAsync(string ProfileURL, Guid? FreelanceProfileId, CancellationToken cancellationToken = default);
    Task<bool> IconExistsAsync(string Icon, Guid? FreelanceProfileId, CancellationToken cancellationToken = default);
    Task<bool> DisplayOrderExistsAsync(byte DisplayOrder, Guid? FreelanceProfileId, CancellationToken cancellationToken = default);
    Task AddAsync(FreelanceProfile FreelanceProfile, CancellationToken cancellationToken = default);
    void Remove(FreelanceProfile FreelanceProfile);
}