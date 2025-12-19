using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CampaignPostRepository : ICampaignPostRepository
{
    private readonly SaasDbContext _context;

    public CampaignPostRepository(SaasDbContext context)
    {
        _context = context;
    }

    public async Task<List<CampaignPost>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Simplified query - no includes for GetAll to prevent issues with empty collections
        return await _context.CampaignPosts
            .ToListAsync(cancellationToken);
    }

    public async Task<CampaignPost?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Include related entities only for single item retrieval
        return await _context.CampaignPosts
            .Include(cp => cp.Campaign)
            .Include(cp => cp.AutomationTasks)
            .FirstOrDefaultAsync(cp => cp.Id == id, cancellationToken);
    }

    public async Task<List<CampaignPost>> GetByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        return await _context.CampaignPosts
            .Where(cp => cp.CampaignId == campaignId)
            .ToListAsync(cancellationToken);
    }

    public async Task<CampaignPost> AddAsync(CampaignPost campaignPost, CancellationToken cancellationToken = default)
    {
        await _context.CampaignPosts.AddAsync(campaignPost, cancellationToken);
        return campaignPost;
    }

    public Task UpdateAsync(CampaignPost campaignPost, CancellationToken cancellationToken = default)
    {
        _context.CampaignPosts.Update(campaignPost);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var campaignPost = await _context.CampaignPosts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(cp => cp.Id == id, cancellationToken);
        
        if (campaignPost == null)
        {
            throw new KeyNotFoundException($"CampaignPost with ID {id} not found.");
        }

        if (campaignPost.IsDeleted)
        {
            throw new InvalidOperationException($"CampaignPost with ID {id} is already deleted.");
        }

        campaignPost.IsDeleted = true;
        campaignPost.DeletedAt = DateTime.UtcNow;
    }

    public async Task<List<CampaignPost>> GetDuePostsAsync(DateTime currentTime, CancellationToken cancellationToken = default)
    {
        return await _context.CampaignPosts
            .Include(cp => cp.Campaign)
            .Include(cp => cp.PlatformPosts)
                .ThenInclude(pp => pp.Platform)
            .Where(cp => 
                cp.ScheduledAt.HasValue && 
                cp.ScheduledAt.Value <= currentTime &&
                cp.PublishStatus == PublishStatus.Pending.ToString())
            .ToListAsync(cancellationToken);
    }
}
