using Application.Common.Interfaces;
using Application.DTOs.OrderProducts;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class OrderProductService : IOrderProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<OrderProductDto>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // Validate that the order exists first
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");
        }

        var orderProducts = await _unitOfWork.OrderProducts.GetByOrderIdAsync(orderId, cancellationToken);
        return _mapper.Map<List<OrderProductDto>>(orderProducts);
    }

    public async Task<OrderProductDto> AddProductToOrderAsync(AddProductToOrderRequest request, CancellationToken cancellationToken = default)
    {
        // Verify order exists
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
        }

        // Verify product exists
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {request.ProductId} not found.");
        }

        // Check if product already exists in order
        var existingOrderProduct = await _unitOfWork.OrderProducts.GetByOrderAndProductIdAsync(request.OrderId, request.ProductId, cancellationToken);
        if (existingOrderProduct != null)
        {
            throw new InvalidOperationException($"Product {request.ProductId} is already in order {request.OrderId}. Use UpdateQuantity instead.");
        }

        var orderProduct = new OrderProduct
        {
            OrderId = request.OrderId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice
        };

        var addedOrderProduct = await _unitOfWork.OrderProducts.AddAsync(orderProduct, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var result = await _unitOfWork.OrderProducts.GetByOrderAndProductIdAsync(request.OrderId, request.ProductId, cancellationToken);
        return _mapper.Map<OrderProductDto>(result);
    }

    public async Task RemoveProductFromOrderAsync(Guid orderId, Guid productId, CancellationToken cancellationToken = default)
    {
        var orderProduct = await _unitOfWork.OrderProducts.GetByOrderAndProductIdAsync(orderId, productId, cancellationToken);
        if (orderProduct == null)
        {
            throw new KeyNotFoundException($"Product {productId} not found in order {orderId}.");
        }

        await _unitOfWork.OrderProducts.DeleteAsync(orderId, productId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderProductDto> UpdateQuantityAsync(Guid orderId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var orderProduct = await _unitOfWork.OrderProducts.GetByOrderAndProductIdAsync(orderId, productId, cancellationToken);
        if (orderProduct == null)
        {
            throw new KeyNotFoundException($"Product {productId} not found in order {orderId}.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0.", nameof(quantity));
        }

        orderProduct.Quantity = quantity;
        await _unitOfWork.OrderProducts.UpdateAsync(orderProduct, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderProductDto>(orderProduct);
    }
}
