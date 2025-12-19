// Application/Services/CampaignService.cs
using Application.Common.Interfaces;
using Application.DTOs.Campaigns;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class CampaignService : ICampaignService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStoreContext _storeContext;
    private IGenericRepository<Campaign> repo => _unitOfWork.GetRepository<Campaign>();

    public CampaignService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ICurrentUserService currentUserService,
        IStoreContext storeContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _storeContext = storeContext;
    }

    /// <summary>
    /// Get all campaigns in current store
    /// STORE-SCOPED - Uses X-Store-ID from header
    /// </summary>
    public async Task<List<CampaignDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // StoreId filtering is handled automatically by EF Core global query filters
        var campaigns = await repo.GetAllAsync(
            c => c.Store,
            c => c.AssignedProduct,
            c => c.CreatedBy
        );

        return _mapper.Map<List<CampaignDto>>(campaigns);
    }

    public async Task<CampaignDto?> GetCampaignByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // StoreId filtering is handled automatically by EF Core global query filters
        var campaign = await repo.GetByIdAsync(id, c => c.Store, c => c.AssignedProduct, c => c.CreatedBy);

        if (campaign == null)
        {
            return null;
        }

        return _mapper.Map<CampaignDto>(campaign);
    }

    public async Task<CampaignDto> CreateCampaignAsync(CreateCampaignRequest request, CancellationToken cancellationToken = default)
    {
        // Get StoreId from StoreContext (set by middleware from X-Store-ID header)
        if (!_storeContext.HasStoreContext)
        {
            throw new InvalidOperationException("StoreId is required for creating a campaign. Ensure X-Store-ID header is provided.");
        }

        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedAccessException("User must be authenticated to create a campaign.");
        }

        // Validate assigned product belongs to the store (if provided)
        if (request.AssignedProductId.HasValue)
        {
            await ValidateProductBelongsToStoreAsync(request.AssignedProductId.Value, _storeContext.StoreId!.Value);
        }

        // Map request to entity
        var campaign = _mapper.Map<Campaign>(request);
        campaign.StoreId = _storeContext.StoreId!.Value; // Auto-inject StoreId
        campaign.CreatedByUserId = _currentUserService.UserId.Value;
        campaign.CreatedAt = DateTime.UtcNow;
        campaign.UpdatedAt = DateTime.UtcNow;

        await repo.AddAsync(campaign);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var savedCampaign = await repo.GetByIdAsync(campaign.Id, c => c.Store, c => c.AssignedProduct, c => c.CreatedBy);

        return _mapper.Map<CampaignDto>(savedCampaign!);
    }

    public async Task<CampaignDto> UpdateCampaignAsync(Guid id, UpdateCampaignRequest request, CancellationToken cancellationToken = default)
    {
        // Get existing campaign (filtered by StoreId automatically)
        var campaign = await repo.GetByIdAsync(id, c => c.Store);

        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID {id} not found.");
        }

        // Validate assigned product if provided
        if (request.AssignedProductId.HasValue)
        {
            await ValidateProductBelongsToStoreAsync(request.AssignedProductId.Value, campaign.StoreId);
        }

        // Map updates
        _mapper.Map(request, campaign);
        campaign.UpdatedAt = DateTime.UtcNow;

        repo.Update(campaign);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var updatedCampaign = await repo.GetByIdAsync(id, c => c.Store, c => c.AssignedProduct, c => c.CreatedBy);

        return _mapper.Map<CampaignDto>(updatedCampaign!);
    }

    public async Task DeleteCampaignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Get existing campaign (filtered by StoreId automatically)
        var campaign = await repo.GetByIdAsync(id, c => c.Store);

        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID {id} not found.");
        }

        // Soft delete
        repo.Delete(campaign);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ===== Private Helper Methods =====

    private async Task ValidateProductBelongsToStoreAsync(Guid productId, Guid storeId)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();
        var product = await productRepo.GetByIdAsync(productId);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {productId} not found.");
        }

        if (product.StoreId != storeId)
        {
            throw new ArgumentException($"Product {productId} does not belong to store {storeId}.");
        }
    }
}