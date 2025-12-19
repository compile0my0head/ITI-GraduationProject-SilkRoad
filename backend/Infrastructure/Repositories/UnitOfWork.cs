using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SaasDbContext _saasDbContext;
        private readonly Dictionary<string, object> _repositories = [];

        private IProductRepository? _products;
        private IOrderRepository? _orders;
        private IOrderProductRepository? _orderProducts;
        private ICustomerRepository? _customers;
        private IStoreRepository? _stores;
        private IUserRepository? _users;
        private ICampaignRepository? _campaigns;
        private ICampaignPostRepository? _campaignPosts;
        private ICampaignPostPlatformRepository? _campaignPostPlatforms;
        private ITeamRepository? _teams;
        private ISocialPlatformRepository? _socialPlatforms;
        private IAutomationTaskRepository? _automationTasks;
        private IChatbotFAQRepository? _chatbotFAQs;

        public UnitOfWork(SaasDbContext saasDbContext)
        {
            _saasDbContext = saasDbContext;
        }

        public IProductRepository Products => _products ??= new ProductRepository(_saasDbContext);

        public IOrderRepository Orders => _orders ??= new OrderRepository(_saasDbContext);

        public IOrderProductRepository OrderProducts => _orderProducts ??= new OrderProductRepository(_saasDbContext);

        public ICustomerRepository Customers => _customers ??= new CustomerRepository(_saasDbContext);

        public IStoreRepository Stores => _stores ??= new StoreRepository(_saasDbContext);

        public IUserRepository Users => _users ??= new UserRepository(_saasDbContext);

        public ICampaignRepository Campaigns => _campaigns ??= new CampaignRepository(_saasDbContext);

        public ICampaignPostRepository CampaignPosts => _campaignPosts ??= new CampaignPostRepository(_saasDbContext);

        public ICampaignPostPlatformRepository CampaignPostPlatforms => _campaignPostPlatforms ??= new CampaignPostPlatformRepository(_saasDbContext);

        public ITeamRepository Teams => _teams ??= new TeamRepository(_saasDbContext);

        public ISocialPlatformRepository SocialPlatforms => _socialPlatforms ??= new SocialPlatformRepository(_saasDbContext);

        public IAutomationTaskRepository AutomationTasks => _automationTasks ??= new AutomationTaskRepository(_saasDbContext);

        public IChatbotFAQRepository ChatbotFAQs => _chatbotFAQs ??= new ChatbotFAQRepository(_saasDbContext);

        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity
        {
            var typeName = typeof(TEntity).Name;

            if (_repositories.TryGetValue(typeName, out object? repository))
            {
                return (IGenericRepository<TEntity>)repository;
            }
            else
            {
                var repo = new GenericRepository<TEntity>(_saasDbContext);
                _repositories[typeName] = repo;
                return repo;
            }
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _saasDbContext.SaveChangesAsync(cancellationToken);
    }
}
