using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderProductRepository : IOrderProductRepository
{
    private readonly SaasDbContext _context;

    public OrderProductRepository(SaasDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderProduct>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderProducts
            .Include(op => op.Product)
            .Include(op => op.Order)
            .Where(op => op.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderProduct?> GetByOrderAndProductIdAsync(Guid orderId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderProducts
            .Include(op => op.Product)
            .Include(op => op.Order)
            .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId, cancellationToken);
    }

    public async Task<OrderProduct> AddAsync(OrderProduct orderProduct, CancellationToken cancellationToken = default)
    {
        await _context.OrderProducts.AddAsync(orderProduct, cancellationToken);
        return orderProduct;
    }

    public Task UpdateAsync(OrderProduct orderProduct, CancellationToken cancellationToken = default)
    {
        _context.OrderProducts.Update(orderProduct);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid orderId, Guid productId, CancellationToken cancellationToken = default)
    {
        var orderProduct = await _context.OrderProducts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId, cancellationToken);
        
        if (orderProduct == null)
        {
            throw new KeyNotFoundException($"OrderProduct not found for OrderId {orderId} and ProductId {productId}.");
        }

        if (orderProduct.IsDeleted)
        {
            throw new InvalidOperationException($"OrderProduct for OrderId {orderId} and ProductId {productId} is already deleted.");
        }

        orderProduct.IsDeleted = true;
        orderProduct.DeletedAt = DateTime.UtcNow;
    }
}
