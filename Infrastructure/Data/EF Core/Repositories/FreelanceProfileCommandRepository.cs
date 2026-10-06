using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.EF_Core.Repositories;

internal sealed class FreelanceProfileCommandRepository : IFreelanceProfileCommandRepository
{
    private readonly AppDbContext _dbContext;

    public FreelanceProfileCommandRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(FreelanceProfile FreelanceProfile, CancellationToken cancellationToken = default)
    {
         await _dbContext.FreelanceProfiles.AddAsync(FreelanceProfile, cancellationToken);
    }

    public Task<bool> DisplayOrderExistsAsync(byte DisplayOrder, Guid? FreelanceProfileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.FreelanceProfiles
            .AnyAsync(fp => fp.DisplayOrder == DisplayOrder && fp.Id != FreelanceProfileId, cancellationToken);
    }

    public Task<FreelanceProfile?> GetByIdAsync(Guid FreelanceProfileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.FreelanceProfiles
            .FirstOrDefaultAsync(fp => fp.Id == FreelanceProfileId, cancellationToken);
    }

    public Task<bool> IconExistsAsync(string Icon, Guid? FreelanceProfileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.FreelanceProfiles
            .AnyAsync(fp => fp.Icon == Icon && fp.Id != FreelanceProfileId, cancellationToken);
    }

    public Task<bool> PlatformNameExistsAsync(string PlatformName, Guid? FreelanceProfileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.FreelanceProfiles
            .AnyAsync(fp => fp.PlatformName == PlatformName && fp.Id != FreelanceProfileId, cancellationToken);
    }

    public Task<bool> ProfileURLExistsAsync(string ProfileURL, Guid? FreelanceProfileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.FreelanceProfiles
            .AnyAsync(fp => fp.ProfileURL == ProfileURL && fp.Id != FreelanceProfileId, cancellationToken);
    }

    public void Remove(FreelanceProfile FreelanceProfile)
    {
        _dbContext.FreelanceProfiles.Remove(FreelanceProfile);
    }
}
