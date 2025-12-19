using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IOrderProductRepository
{
    Task<List<OrderProduct>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderProduct?> GetByOrderAndProductIdAsync(Guid orderId, Guid productId, CancellationToken cancellationToken = default);
    Task<OrderProduct> AddAsync(OrderProduct orderProduct, CancellationToken cancellationToken = default);
    Task UpdateAsync(OrderProduct orderProduct, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid orderId, Guid productId, CancellationToken cancellationToken = default);
}
