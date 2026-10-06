using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.EF_Core.Repositories;

internal sealed class AboutItemCommandRepository
    : IAboutItemCommandRepository
{
    private readonly AppDbContext _context;

    public AboutItemCommandRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AboutItem?> GetByIdAsync(
        Guid aboutItemId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AboutItems
            .FirstOrDefaultAsync(
                ai => ai.Id == aboutItemId,
                cancellationToken);
    }

    public async Task<bool> TitleExistsAsync(
        string title,
        Guid? aboutItemId = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.AboutItems
            .AnyAsync(
                ai => ai.Title == title &&
                      (!aboutItemId.HasValue ||
                       ai.Id != aboutItemId.Value),
                cancellationToken);
    }

    public async Task<bool> DisplayOrderExistsAsync(
        byte displayOrder,
        Guid? aboutItemId = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.AboutItems
            .AnyAsync(
                ai => ai.DisplayOrder == displayOrder &&
                      (!aboutItemId.HasValue ||
                       ai.Id != aboutItemId.Value),
                cancellationToken);
    }

    public async Task AddAsync(
        AboutItem aboutItem,
        CancellationToken cancellationToken = default)
    {
        await _context.AboutItems.AddAsync(
            aboutItem,
            cancellationToken);
    }

    public void Remove(AboutItem aboutItem)
    {
        _context.AboutItems.Remove(aboutItem);
    }
}