using Application.Common.Interfaces;
using Application.DTOs.Stores;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class StoreService : IStoreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStoreAuthorizationService _storeAuthorizationService;
    private IGenericRepository<Store> repo => _unitOfWork.GetRepository<Store>();
    
    public StoreService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ICurrentUserService currentUserService,
        IStoreAuthorizationService storeAuthorizationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _storeAuthorizationService = storeAuthorizationService;
    }

    public async Task<StoreDto> CreateStoreAsync(CreateStoreRequest request, CancellationToken cancellationToken = default)
    {
        // Map request to Store entity
        var store = _mapper.Map<Store>(request);
        
        // Get OwnerUserId from authenticated user (from JWT token)
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedAccessException("User must be authenticated to create a store.");
        }
        
        store.OwnerUserId = _currentUserService.UserId.Value;
        store.CreatedAt = DateTime.UtcNow;

        await repo.AddAsync(store);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StoreDto>(store);
    }

    public async Task DeleteStoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var store = await repo.GetByIdAsync(id);
        
        if (store == null)
        {
            throw new KeyNotFoundException($"Store with ID {id} not found.");
        }
        
        // Check if current user is the owner
        if (_currentUserService.UserId != null && store.OwnerUserId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own stores.");
        }
        
        // Soft delete
        repo.Delete(store);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<StoreDto>> GetAllStoresAsync(CancellationToken cancellationToken = default)
    {
        var stores = await repo.GetAllAsync();
        return _mapper.Map<List<StoreDto>>(stores);
    }

    public async Task<List<StoreDto>> GetMyStoresAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedAccessException("User must be authenticated to view their stores.");
        }

        var userId = _currentUserService.UserId.Value;
        
        // Get all store IDs user has access to (owned + team member)
        var storeIds = await _storeAuthorizationService.GetUserStoreIdsAsync(userId, cancellationToken);
        
        // Get all stores user has access to
        var allStores = await repo.GetAllAsync();
        var userStores = allStores.Where(s => storeIds.Contains(s.Id)).ToList();
        
        return _mapper.Map<List<StoreDto>>(userStores);
    }

    public async Task<StoreDto?> GetStoreByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var store = await repo.GetByIdAsync(id);
        
        if (store == null)
        {
            return null;
        }
        
        return _mapper.Map<StoreDto>(store);
    }

    public async Task<StoreDto> UpdateStoreAsync(Guid id, UpdateStoreRequest request, CancellationToken cancellationToken = default)
    {
        var store = await repo.GetByIdAsync(id);
        
        if (store == null)
        {
            throw new KeyNotFoundException($"Store with ID {id} not found.");
        }
        
        // Check if current user is the owner (only owner can update store details)
        if (_currentUserService.UserId != null && store.OwnerUserId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Only the store owner can update store details.");
        }
        
        _mapper.Map(request, store);
        
        repo.Update(store);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<StoreDto>(store);
    }
}
