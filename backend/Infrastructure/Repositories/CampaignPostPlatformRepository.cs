using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repository for CampaignPostPlatform entity operations
/// Guarantees all required navigation properties are loaded for publishing operations
/// </summary>
public class CampaignPostPlatformRepository : ICampaignPostPlatformRepository
{
    private readonly SaasDbContext _context;

    public CampaignPostPlatformRepository(SaasDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all due platform posts with fully-hydrated navigation properties.
    /// Repository contract GUARANTEES: CampaignPost, Campaign, and Platform are loaded.
    /// </summary>
    public async Task<List<CampaignPostPlatform>> GetDuePlatformPostsAsync(
        DateTime currentTime, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<CampaignPostPlatform>()
            .Include(pp => pp.CampaignPost)
                .ThenInclude(cp => cp.Campaign)
            .Include(pp => pp.Platform)
            .Where(pp => 
                pp.ScheduledAt <= currentTime &&
                pp.PublishStatus == PublishStatus.Pending.ToString())
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all CampaignPostPlatform records with navigation properties
    /// </summary>
    public async Task<List<CampaignPostPlatform>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<CampaignPostPlatform>()
            .Include(pp => pp.CampaignPost)
                .ThenInclude(cp => cp.Campaign)
            .Include(pp => pp.Platform)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all CampaignPostPlatform records for a specific CampaignPost
    /// </summary>
    public async Task<List<CampaignPostPlatform>> GetByCampaignPostIdAsync(
        Guid campaignPostId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<CampaignPostPlatform>()
            .Include(pp => pp.CampaignPost)
                .ThenInclude(cp => cp.Campaign)
            .Include(pp => pp.Platform)
            .Where(pp => pp.CampaignPostId == campaignPostId)
            .ToListAsync(cancellationToken);
    }

    public async Task<CampaignPostPlatform?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<CampaignPostPlatform>()
            .Include(pp => pp.CampaignPost)
            .Include(pp => pp.Platform)
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);
    }

    public Task UpdateAsync(
        CampaignPostPlatform platformPost, 
        CancellationToken cancellationToken = default)
    {
        _context.Set<CampaignPostPlatform>().Update(platformPost);
        return Task.CompletedTask;
    }
}
