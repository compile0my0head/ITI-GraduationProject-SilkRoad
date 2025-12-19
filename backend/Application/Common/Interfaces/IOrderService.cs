using Application.DTOs.Orders;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IOrderService
{
    Task<List<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<OrderDto>> GetAllAsync(OrderStatus? status, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<OrderDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Order lifecycle methods
    Task<OrderDto> AcceptOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderDto> RejectOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}
