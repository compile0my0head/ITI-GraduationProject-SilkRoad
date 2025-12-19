using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly SaasDbContext _context;

    public CustomerRepository(SaasDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Simplified query - no includes for GetAll to prevent issues with empty collections
        return await _context.Customers
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Include related entities only for single item retrieval
        return await _context.Customers
            .Include(c => c.Store)
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Customer>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Where(c => c.StoreId == storeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByPSIDAsync(string psid, Guid storeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(psid))
            return null;

        return await _context.Customers
            .FirstOrDefaultAsync(c => c.PSID == psid && c.StoreId == storeId, cancellationToken);
    }

    public async Task<Customer?> GetByPhoneAsync(string phone, Guid storeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Phone == phone && c.StoreId == storeId, cancellationToken);
    }

    public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
        return customer;
    }

    public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _context.Customers.Update(customer);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {id} not found.");
        }

        if (customer.IsDeleted)
        {
            throw new InvalidOperationException($"Customer with ID {id} is already deleted.");
        }

        customer.IsDeleted = true;
        customer.DeletedAt = DateTime.UtcNow;
    }
}
