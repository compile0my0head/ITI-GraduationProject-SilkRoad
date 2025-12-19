using Application.Common.Configuration;
using Application.Common.Interfaces;
using Application.Services;
using Application.Services.Publishing;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IServiceManager, ServiceManager>();

        // Register AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // Register JWT Options
        services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));

        // Register Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<ICampaignPostService, CampaignPostService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<ITeamMemberService, TeamMemberService>();
        services.AddScoped<ISocialPlatformService, SocialPlatformService>();
        services.AddScoped<IAutomationTaskService, AutomationTaskService>();
        services.AddScoped<IChatbotFAQService, ChatbotFAQService>();
        services.AddScoped<IOrderProductService, OrderProductService>();
        services.AddScoped<IAuthService, AuthService>();

        // Register Publishing Services
        services.AddScoped<IPlatformPublishingService, PlatformPublishingService>();

        // Register Chatbot Order Service
        services.AddScoped<ChatbotOrderService>();

        return services;
    }
}
