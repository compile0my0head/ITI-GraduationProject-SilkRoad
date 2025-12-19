using Application.Common.Interfaces;
using Application.DTOs.Campaigns;
using Application.DTOs.CampaignPosts;
using Application.DTOs.ChatbotFAQs;
using Application.DTOs.Customers;
using Application.DTOs.Orders;
using Application.DTOs.Products;
using Application.DTOs.SocialPlatforms;
using Application.DTOs.Stores;
using Application.DTOs.AutomationTasks;
using Application.DTOs.Teams;
using Application.DTOs.Users;
using Application.DTOs.OrderProducts;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ==================== STORE MAPPINGS ====================
        CreateMap<Store, StoreDto>();

        CreateMap<CreateStoreRequest, Store>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Owner, opt => opt.Ignore())
            .ForMember(dest => dest.Teams, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore())
            .ForMember(dest => dest.Campaigns, opt => opt.Ignore())
            .ForMember(dest => dest.Customers, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore())
            .ForMember(dest => dest.Platforms, opt => opt.Ignore())
            .ForMember(dest => dest.FAQs, opt => opt.Ignore())
            .ForMember(dest => dest.AutomationTasks, opt => opt.Ignore());

        CreateMap<UpdateStoreRequest, Store>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ==================== PRODUCT MAPPINGS ====================
        CreateMap<Product, ProductDto>();

        CreateMap<CreateProductRequest, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Store, opt => opt.Ignore())
            .ForMember(dest => dest.OrderProducts, opt => opt.Ignore())
            .ForMember(dest => dest.Campaigns, opt => opt.Ignore());

        CreateMap<UpdateProductRequest, Product>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ==================== CAMPAIGN MAPPINGS ====================
        CreateMap<Campaign, CampaignDto>()
            .ForMember(dest => dest.CampaignStage, opt => opt.MapFrom(src => src.CampaignStage.ToString()))
            .ForMember(dest => dest.AssignedProductName, opt => opt.MapFrom(src => src.AssignedProduct != null ? src.AssignedProduct.ProductName : null))
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.FullName : null));

        CreateMap<CreateCampaignRequest, Campaign>()
            .ForMember(dest => dest.CampaignStage, opt => opt.MapFrom(src => 
                string.IsNullOrEmpty(src.CampaignStage) ? CampaignStage.Draft : Enum.Parse<CampaignStage>(src.CampaignStage)))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Store, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedProduct, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Posts, opt => opt.Ignore());

        CreateMap<UpdateCampaignRequest, Campaign>()
            .ForMember(dest => dest.CampaignStage, opt => opt.MapFrom((src, dest) =>
                string.IsNullOrEmpty(src.CampaignStage) ? dest.CampaignStage : Enum.Parse<CampaignStage>(src.CampaignStage)))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ==================== CAMPAIGN POST MAPPINGS ====================
        CreateMap<CampaignPost, CampaignPostDto>()
            .ForMember(dest => dest.CampaignName, opt => opt.MapFrom(src => src.Campaign != null ? src.Campaign.CampaignName : string.Empty));

        CreateMap<CreateCampaignPostRequest, CampaignPost>()
            .ForMember(dest => dest.PublishStatus, opt => opt.Ignore())
            .ForMember(dest => dest.PublishedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastPublishError, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Campaign, opt => opt.Ignore())
            .ForMember(dest => dest.AutomationTasks, opt => opt.Ignore())
            .ForMember(dest => dest.PlatformPosts, opt => opt.Ignore());

        CreateMap<UpdateCampaignPostRequest, CampaignPost>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ==================== CUSTOMER MAPPINGS ====================
        CreateMap<Customer, CustomerDto>();

        CreateMap<CreateCustomerRequest, Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Store, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore());

        CreateMap<UpdateCustomerRequest, Customer>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ==================== ORDER MAPPINGS ====================
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustomerName : string.Empty))
            .ForMember(dest => dest.StatusDisplayName, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateOrderRequest, Order>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.Pending))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Store, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.OrderProducts, opt => opt.Ignore());

        CreateMap<UpdateOrderRequest, Order>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ==================== CHATBOT FAQ MAPPINGS ====================
        CreateMap<ChatbotFAQ, ChatbotFAQDto>()
            .ForMember(dest => dest.MessageType, opt => opt.MapFrom(src => src.MessageType.ToString()));

        CreateMap<CreateChatbotFAQRequest, ChatbotFAQ>()
            .ForMember(dest => dest.MessageType, opt => opt.MapFrom(src => Enum.Parse<MessageType>(src.MessageType)))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Store, opt => opt.Ignore());

        CreateMap<UpdateChatbotFAQRequest, ChatbotFAQ>()
            .ForMember(dest => dest.MessageType, opt => opt.MapFrom((src, dest) =>
                string.IsNullOrEmpty(src.MessageType) ? dest.MessageType : Enum.Parse<MessageType>(src.MessageType)))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ==================== SOCIAL PLATFORM MAPPINGS ====================
        CreateMap<SocialPlatform, SocialPlatformDto>()
            .ForMember(dest => dest.PlatformName, opt => opt.MapFrom(src => src.PlatformName.ToString()))
            .ForMember(dest => dest.ExternalPageID, opt => opt.MapFrom(src => src.ExternalPageID))
            .ForMember(dest => dest.PageName, opt => opt.MapFrom(src => src.PageName));

        CreateMap<CreateSocialPlatformRequest, SocialPlatform>()
            .ForMember(dest => dest.PlatformName, opt => opt.MapFrom(src => Enum.Parse<PlatformName>(src.PlatformName)))
            .ForMember(dest => dest.ExternalPageID, opt => opt.MapFrom(src => src.ExternalPageID))
            .ForMember(dest => dest.PageName, opt => opt.MapFrom(src => src.PageName))
            .ForMember(dest => dest.AccessToken, opt => opt.MapFrom(src => src.AccessToken))
            .ForMember(dest => dest.IsConnected, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Store, opt => opt.Ignore());

        CreateMap<UpdateSocialPlatformRequest, SocialPlatform>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ==================== AUTOMATION TASK MAPPINGS ====================
        CreateMap<AutomationTask, AutomationTaskDto>()
            .ForMember(dest => dest.TaskType, opt => opt.MapFrom(src => src.TaskType.ToString()));

        CreateMap<CreateAutomationTaskRequest, AutomationTask>()
            .ForMember(dest => dest.TaskType, opt => opt.MapFrom(src => Enum.Parse<TaskType>(src.TaskType)))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Store, opt => opt.Ignore())
            .ForMember(dest => dest.RelatedCampaignPost, opt => opt.Ignore());

        CreateMap<UpdateAutomationTaskRequest, AutomationTask>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ==================== TEAM MAPPINGS ====================
        CreateMap<Team, TeamDto>()
            .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Members.Count));

        CreateMap<CreateTeamRequest, Team>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Store, opt => opt.Ignore())
            .ForMember(dest => dest.Members, opt => opt.Ignore());

        CreateMap<UpdateTeamRequest, Team>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<TeamMember, TeamMemberDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        // ==================== USER MAPPINGS ====================
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.OwnedStoresCount, opt => opt.MapFrom(src => src.OwnedStores.Count));

        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
            .ForMember(dest => dest.OwnedStores, opt => opt.Ignore())
            .ForMember(dest => dest.TeamMemberships, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore());

        CreateMap<UpdateUserRequest, User>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ==================== ORDER PRODUCT MAPPINGS ====================
        CreateMap<OrderProduct, OrderProductDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName));
    }
}
