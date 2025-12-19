using Application.Common.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IStoreService> _storeService;
    private readonly Lazy<ICampaignService> _campaignService;
    private readonly Lazy<ICampaignPostService> _campaignPostService;
    private readonly Lazy<ISocialPlatformService> _socialPlatformService;

    public ServiceManager(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ICurrentUserService currentUserService,
        IStoreContext storeContext,
        IStoreAuthorizationService storeAuthorizationService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _storeService = new Lazy<IStoreService>(() => new StoreService(unitOfWork, mapper, currentUserService, storeAuthorizationService));
        _campaignService = new Lazy<ICampaignService>(() => new CampaignService(unitOfWork, mapper, currentUserService, storeContext));
        _campaignPostService = new Lazy<ICampaignPostService>(() => new CampaignPostService(unitOfWork, mapper, currentUserService, storeContext));
        _socialPlatformService = new Lazy<ISocialPlatformService>(() => new SocialPlatformService(unitOfWork, mapper, currentUserService, storeContext, httpClientFactory, configuration));
    }

    public IStoreService StoreService => _storeService.Value;
    public ICampaignService CampaignService => _campaignService.Value;
    public ICampaignPostService CampaignPostService => _campaignPostService.Value;
    public ISocialPlatformService SocialPlatformService => _socialPlatformService.Value;
}