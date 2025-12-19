using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SocialPlatformRepository : ISocialPlatformRepository
{
    private readonly SaasDbContext _context;

    public SocialPlatformRepository(SaasDbContext context)
    {
        _context = context;
    }

    public async Task<List<SocialPlatform>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Simplified query - no includes for GetAll to prevent issues with empty collections
        return await _context.SocialPlatforms
            .ToListAsync(cancellationToken);
    }

    public async Task<SocialPlatform?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Include related entities only for single item retrieval
        return await _context.SocialPlatforms
            .Include(sp => sp.Store)
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
    }

    public async Task<List<SocialPlatform>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        return await _context.SocialPlatforms
            .Where(sp => sp.StoreId == storeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SocialPlatform>> GetConnectedPlatformsByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        return await _context.SocialPlatforms
            .Where(sp => sp.StoreId == storeId && sp.IsConnected)
            .ToListAsync(cancellationToken);
    }

    public async Task<SocialPlatform?> GetByExternalPageIdAsync(string externalPageId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalPageId))
            return null;

        return await _context.SocialPlatforms
            .Include(sp => sp.Store)
            .FirstOrDefaultAsync(sp => sp.ExternalPageID == externalPageId, cancellationToken);
    }

    public async Task<SocialPlatform> AddAsync(SocialPlatform socialPlatform, CancellationToken cancellationToken = default)
    {
        await _context.SocialPlatforms.AddAsync(socialPlatform, cancellationToken);
        return socialPlatform;
    }

    public Task UpdateAsync(SocialPlatform socialPlatform, CancellationToken cancellationToken = default)
    {
        _context.SocialPlatforms.Update(socialPlatform);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var socialPlatform = await _context.SocialPlatforms
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
        
        if (socialPlatform == null)
        {
            throw new KeyNotFoundException($"SocialPlatform with ID {id} not found.");
        }

        if (socialPlatform.IsDeleted)
        {
            throw new InvalidOperationException($"SocialPlatform with ID {id} is already deleted.");
        }

        socialPlatform.IsDeleted = true;
        socialPlatform.DeletedAt = DateTime.UtcNow;
    }
}
