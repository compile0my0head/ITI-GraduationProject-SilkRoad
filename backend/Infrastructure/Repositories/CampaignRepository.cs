namespace Infrastructure.Repositories;

using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class CampaignRepository : ICampaignRepository
{
    private readonly SaasDbContext _context;

    public CampaignRepository(SaasDbContext context)
    {
        _context = context;
    }

    public async Task<List<Campaign>> GetAllByStoreIdAsync(Guid storeId)
    {
        return await _context.Campaigns
            .Where(c => c.StoreId == storeId)
            .Include(c => c.Posts) // Eager loading for common access
            .ToListAsync();
    }

    public async Task<Campaign?> GetByIdAsync(Guid id)
    {
        return await _context.Campaigns
            .Include(c => c.Posts)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Campaign campaign)
    {
        await _context.Campaigns.AddAsync(campaign);
    }

    public void Update(Campaign campaign)
    {
        _context.Campaigns.Update(campaign);
    }

    public void Delete(Campaign campaign)
    {
        campaign.IsDeleted = true;
        campaign.DeletedAt = DateTime.UtcNow;
        _context.Campaigns.Update(campaign);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}