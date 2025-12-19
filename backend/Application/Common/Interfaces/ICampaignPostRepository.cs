using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICampaignPostRepository
{
    Task<List<CampaignPost>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CampaignPost?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<CampaignPost>> GetByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default);
    Task<CampaignPost> AddAsync(CampaignPost campaignPost, CancellationToken cancellationToken = default);
    Task UpdateAsync(CampaignPost campaignPost, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<CampaignPost>> GetDuePostsAsync(DateTime currentTime, CancellationToken cancellationToken = default);
}
