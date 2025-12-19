using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly SaasDbContext _context;

    public OrderRepository(SaasDbContext context)
    {
        _context = context;
    }

    public async Task<List<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Include Customer navigation property so CustomerName can be populated in DTO
        return await _context.Orders
            .Include(o => o.Customer)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Include related entities only for single item retrieval
        return await _context.Orders
            .Include(o => o.Store)
            .Include(o => o.Customer)
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<List<Order>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.StoreId == storeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        return order;
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found.");
        }

        if (order.IsDeleted)
        {
            throw new InvalidOperationException($"Order with ID {id} is already deleted.");
        }

        order.IsDeleted = true;
        order.DeletedAt = DateTime.UtcNow;
    }
}
