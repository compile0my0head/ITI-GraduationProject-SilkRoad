using Application.Common.Interfaces;
using Application.DTOs.Orders;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStoreContext _storeContext;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IStoreContext storeContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storeContext = storeContext;
    }

    public async Task<List<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // StoreId filtering is handled automatically by EF Core global query filters
        var orders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<List<OrderDto>> GetAllAsync(OrderStatus? status, CancellationToken cancellationToken = default)
    {
        // StoreId filtering is handled automatically by EF Core global query filters
        var orders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);
        
        // Filter by status if provided
        if (status.HasValue)
        {
            orders = orders.Where(o => o.Status == status.Value).ToList();
        }
        
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        return order == null ? null : _mapper.Map<OrderDto>(order);
    }

    public async Task<List<OrderDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(customerId, cancellationToken);
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        // Get StoreId from StoreContext (set by middleware from X-Store-ID header)
        if (!_storeContext.HasStoreContext)
        {
            throw new InvalidOperationException("StoreId is required for creating an order. Ensure X-Store-ID header is provided.");
        }

        // Validate that customer exists and belongs to the store
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {request.CustomerId} not found.");
        }

        if (customer.StoreId != _storeContext.StoreId!.Value)
        {
            throw new InvalidOperationException($"Customer {request.CustomerId} does not belong to the current store.");
        }

        var order = _mapper.Map<Order>(request);
        order.StoreId = _storeContext.StoreId!.Value; // Auto-inject StoreId
        order.Status = OrderStatus.Pending; // Explicitly set default status
        
        var createdOrder = await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Reload with navigation properties to return complete data
        var savedOrder = await _unitOfWork.Orders.GetByIdAsync(createdOrder.Id, cancellationToken);
        
        return _mapper.Map<OrderDto>(savedOrder);
    }

    public async Task<OrderDto> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found.");
        }

        _mapper.Map(request, order);
        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Reload with navigation properties to return complete data
        var updatedOrder = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        
        return _mapper.Map<OrderDto>(updatedOrder!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Orders.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ==================== ORDER LIFECYCLE METHODS ====================

    public async Task<OrderDto> AcceptOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");
        }

        // Validate order is in Pending status
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot accept order. Order status is '{order.Status}'. Only orders with 'Pending' status can be accepted.");
        }

        // Update status to Accepted
        order.Status = OrderStatus.Accepted;
        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Reload with navigation properties
        var updatedOrder = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        
        return _mapper.Map<OrderDto>(updatedOrder!);
    }

    public async Task<OrderDto> RejectOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");
        }

        // Validate order is in Pending status
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot reject order. Order status is '{order.Status}'. Only orders with 'Pending' status can be rejected.");
        }

        // Update status to Rejected
        order.Status = OrderStatus.Rejected;
        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Reload with navigation properties
        var updatedOrder = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        
        return _mapper.Map<OrderDto>(updatedOrder!);
    }
}
