using Application.DTOs.OrderProducts;

namespace Application.Common.Interfaces;

public interface IOrderProductService
{
    Task<List<OrderProductDto>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderProductDto> AddProductToOrderAsync(AddProductToOrderRequest request, CancellationToken cancellationToken = default);
    Task RemoveProductFromOrderAsync(Guid orderId, Guid productId, CancellationToken cancellationToken = default);
    Task<OrderProductDto> UpdateQuantityAsync(Guid orderId, Guid productId, int quantity, CancellationToken cancellationToken = default);
}
