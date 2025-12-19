using Application.Common.Interfaces;
using Application.DTOs.Publishing;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    /// <summary>
    /// DEPRECATED: This service was used for the old publishing approach.
    /// 
    /// Campaign scheduling validation is now handled directly in PlatformPublishingService.
    /// 
    /// This class should be removed in a future cleanup, but is kept temporarily for reference.
    /// DO NOT use this service for publishing - it causes duplicate posts!
    /// </summary>
    [Obsolete("This service causes duplicate publishing. Campaign scheduling is now handled in PlatformPublishingService.")]
    public class CampaignSchedulingService : ICampaignSchedulingService
    {
        private readonly ICampaignPostRepository _campaignPostRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IEnumerable<ISocialPlatformPublisher> _publishers;
        private readonly ILogger<CampaignSchedulingService> _logger;

        public CampaignSchedulingService(
            ICampaignPostRepository campaignPostRepository,
            ICampaignRepository campaignRepository,
            IEnumerable<ISocialPlatformPublisher> publishers,
            ILogger<CampaignSchedulingService> logger)
        {
            _campaignPostRepository = campaignPostRepository;
            _campaignRepository = campaignRepository;
            _publishers = publishers;
            _logger = logger;
        }

        [Obsolete("DO NOT USE - Causes duplicate publishing. Use PlatformPublishingService instead.")]
        public async Task ProcessDueCampaignPostsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning(
                "⚠️ CampaignSchedulingService.ProcessDueCampaignPostsAsync() was called! " +
                "This is DEPRECATED and causes duplicate publishing. " +
                "Use PlatformPublishingService instead.");
            
            // DO NOT execute any publishing logic
            throw new InvalidOperationException(
                "CampaignSchedulingService is deprecated. " +
                "Campaign scheduling is now handled in PlatformPublishingService. " +
                "Remove any Hangfire jobs calling this service.");
        }
    }
}
