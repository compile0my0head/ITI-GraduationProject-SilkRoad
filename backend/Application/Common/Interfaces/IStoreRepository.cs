using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IStoreRepository
{
    Task<List<Store>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Store>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<Store> AddAsync(Store store, CancellationToken cancellationToken = default);
    Task UpdateAsync(Store store, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
