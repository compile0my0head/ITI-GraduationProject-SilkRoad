using Application.DTOs.Campaigns;

namespace Application.Common.Interfaces;

public interface ICampaignService
{
    // STORE-SCOPED - Uses X-Store-ID from header
    Task<List<CampaignDto>> GetAllAsync(CancellationToken cancellationToken = default);
    
    Task<CampaignDto?> GetCampaignByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CampaignDto> CreateCampaignAsync(CreateCampaignRequest request, CancellationToken cancellationToken = default);

    Task<CampaignDto> UpdateCampaignAsync(Guid id, UpdateCampaignRequest request, CancellationToken cancellationToken = default);

    Task DeleteCampaignAsync(Guid id, CancellationToken cancellationToken = default);
}
