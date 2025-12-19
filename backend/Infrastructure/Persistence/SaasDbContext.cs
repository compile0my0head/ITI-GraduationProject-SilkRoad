using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class SaasDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    private readonly IStoreContext? _storeContext;

    public SaasDbContext(DbContextOptions<SaasDbContext> options)
        : base(options)
    {
    }

    public SaasDbContext(DbContextOptions<SaasDbContext> options, IStoreContext storeContext)
        : base(options)
    {
        _storeContext = storeContext;
    }

    // Application DbSets
    public DbSet<Store> Stores { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Team> Teams { get; set; } = null!;
    public DbSet<TeamMember> TeamMembers { get; set; } = null!;
    public DbSet<Campaign> Campaigns { get; set; } = null!;
    public DbSet<CampaignPost> CampaignPosts { get; set; } = null!;
    public DbSet<CampaignPostPlatform> CampaignPostPlatforms { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderProduct> OrderProducts { get; set; } = null!;
    public DbSet<SocialPlatform> SocialPlatforms { get; set; } = null!;
    public DbSet<ChatbotFAQ> ChatbotFAQs { get; set; } = null!;
    public DbSet<AutomationTask> AutomationTasks { get; set; } = null!;
    
    // Authentication DbSet (JWT refresh tokens)
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SaasDbContext).Assembly);
        
        // ==================== GLOBAL QUERY FILTERS ====================
        
        // Store-scoped entities: Apply BOTH soft delete AND store context filters
        // Note: Store entity itself is NOT filtered by StoreId (users can see all their stores)
        
        modelBuilder.Entity<Product>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_storeContext == null || !_storeContext.HasStoreContext || e.StoreId == _storeContext.StoreId));
        
        modelBuilder.Entity<Campaign>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_storeContext == null || !_storeContext.HasStoreContext || e.StoreId == _storeContext.StoreId));
        
        modelBuilder.Entity<CampaignPost>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_storeContext == null || !_storeContext.HasStoreContext || e.Campaign.StoreId == _storeContext.StoreId));
        
        modelBuilder.Entity<CampaignPostPlatform>().HasQueryFilter(e => 
            (_storeContext == null || !_storeContext.HasStoreContext || e.CampaignPost.Campaign.StoreId == _storeContext.StoreId));
        
        modelBuilder.Entity<Customer>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_storeContext == null || !_storeContext.HasStoreContext || e.StoreId == _storeContext.StoreId));
        
        modelBuilder.Entity<Order>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_storeContext == null || !_storeContext.HasStoreContext || e.StoreId == _storeContext.StoreId));
        
        modelBuilder.Entity<OrderProduct>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_storeContext == null || !_storeContext.HasStoreContext || e.Order.StoreId == _storeContext.StoreId));
        
        modelBuilder.Entity<Team>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_storeContext == null || !_storeContext.HasStoreContext || e.StoreId == _storeContext.StoreId));
        
        modelBuilder.Entity<SocialPlatform>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_storeContext == null || !_storeContext.HasStoreContext || e.StoreId == _storeContext.StoreId));
        
        modelBuilder.Entity<ChatbotFAQ>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_storeContext == null || !_storeContext.HasStoreContext || e.StoreId == _storeContext.StoreId));
        
        modelBuilder.Entity<AutomationTask>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_storeContext == null || !_storeContext.HasStoreContext || e.StoreId == _storeContext.StoreId));
        
        // Non-store-scoped entities: Apply ONLY soft delete filter
        modelBuilder.Entity<Store>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TeamMember>().HasQueryFilter(e => !e.IsDeleted);
    }
}

