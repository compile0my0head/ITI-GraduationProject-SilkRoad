using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICampaignRepository
{
    Task<List<Campaign>> GetAllByStoreIdAsync(Guid storeId);
    Task<Campaign?> GetByIdAsync(Guid id);
    Task AddAsync(Campaign campaign);
    void Update(Campaign campaign);
    void Delete(Campaign campaign);
    Task<int> SaveChangesAsync();
}