using Application.DTOs.Stores;

namespace Application.Common.Interfaces;

public interface IStoreService
{
    Task<List<StoreDto>> GetAllStoresAsync(CancellationToken cancellationToken = default);
    Task<List<StoreDto>> GetMyStoresAsync(CancellationToken cancellationToken = default);
    Task<StoreDto?> GetStoreByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StoreDto> CreateStoreAsync(CreateStoreRequest request, CancellationToken cancellationToken = default);
    Task<StoreDto> UpdateStoreAsync(Guid id, UpdateStoreRequest request, CancellationToken cancellationToken = default);
    Task DeleteStoreAsync(Guid id, CancellationToken cancellationToken = default);
}
