using Domain.Common;
using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern - Manages transactions across multiple repositories
/// Ensures all database changes are committed or rolled back together
/// </summary>
public interface IUnitOfWork 
{
    // Repository access properties
    IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity;
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IOrderProductRepository OrderProducts { get; }
    ICustomerRepository Customers { get; }
    IStoreRepository Stores { get; }
    IUserRepository Users { get; }
    ICampaignRepository Campaigns { get; }
    ICampaignPostRepository CampaignPosts { get; }
    ICampaignPostPlatformRepository CampaignPostPlatforms { get; }
    ITeamRepository Teams { get; }
    ISocialPlatformRepository SocialPlatforms { get; }
    IAutomationTaskRepository AutomationTasks { get; }
    IChatbotFAQRepository ChatbotFAQs { get; }


    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    //Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    //Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    //Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
