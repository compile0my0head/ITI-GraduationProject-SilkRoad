using Application.Common.Interfaces;
using Application.DTOs.Products;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

/// <summary>
/// Product Service - Implementation using Unit of Work pattern
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStoreContext _storeContext;
    private readonly IProductEmbeddingService _embeddingService;

    public ProductService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IStoreContext storeContext,
        IProductEmbeddingService embeddingService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storeContext = storeContext;
        _embeddingService = embeddingService;
    }

    public async Task<GetProductsResponse> GetAllAsync(bool? inStockOnly, CancellationToken cancellationToken = default)
    {
        // Note: StoreId filtering is now handled automatically by EF Core global query filters
        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);

        if (inStockOnly == true)
        {
            products = products.Where(p => p.InStock).ToList();
        }

        var productDtos = _mapper.Map<List<ProductDto>>(products);

        return new GetProductsResponse
        {
            Products = productDtos,
            TotalCount = productDtos.Count,
            Message = $"Successfully retrieved {productDtos.Count} product(s)"
        };
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        // Get StoreId from StoreContext (set by middleware from X-Store-ID header)
        if (!_storeContext.HasStoreContext)
        {
            throw new InvalidOperationException("StoreId is required for creating a product. Ensure X-Store-ID header is provided.");
        }

        var product = _mapper.Map<Product>(request);
        product.StoreId = _storeContext.StoreId!.Value; // Auto-inject StoreId
        
        var createdProduct = await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Send to embedding webhook (non-blocking, fire-and-forget)
        _ = Task.Run(async () => await _embeddingService.EmbedProductAsync(createdProduct, CancellationToken.None));
        
        return _mapper.Map<ProductDto>(createdProduct);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        _mapper.Map(request, product);
        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Send updated product to embedding webhook (non-blocking)
        _ = Task.Run(async () => await _embeddingService.EmbedProductAsync(product, CancellationToken.None));
        
        var updatedProduct = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<ProductDto>(updatedProduct!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Products.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

