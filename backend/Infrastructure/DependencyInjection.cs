using Application.Common.Configuration;
using Application.Common.Interfaces;
using Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register HTTP Context Accessor (needed for ICurrentUserService)
        services.AddHttpContextAccessor();

        // Register HttpClientFactory for external API calls (Facebook, Instagram, embedding, etc.)
        services.AddHttpClient();

        // Register Current User Service (gets authenticated user from JWT token)
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ? Register Store Context (scoped - one instance per HTTP request)
        services.AddScoped<StoreContext>();
        services.AddScoped<IStoreContext>(provider => provider.GetRequiredService<StoreContext>());

        // ? Register Store Authorization Service
        services.AddScoped<IStoreAuthorizationService, StoreAuthorizationService>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register repositories (can still be used directly if needed)
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ICampaignPostRepository, CampaignPostRepository>();
        services.AddScoped<ICampaignPostPlatformRepository, CampaignPostPlatformRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderProductRepository, OrderProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<ISocialPlatformRepository, SocialPlatformRepository>();
        services.AddScoped<IAutomationTaskRepository, AutomationTaskRepository>();
        services.AddScoped<IChatbotFAQRepository, ChatbotFAQRepository>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Register Identity services
        services.AddScoped<IJwtService, JwtOptions>();

        // Register Social Platform Publishers
        services.AddScoped<ISocialPlatformPublisher, FacebookPublisher>();
        // Add more publishers here: InstagramPublisher, TwitterPublisher, etc.

        // Register Product Embedding Service
        services.AddScoped<IProductEmbeddingService, ProductEmbeddingService>();

        return services;
    }
}
