using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly SaasDbContext _context;

    public StoreRepository(SaasDbContext context)
    {
        _context = context;
    }

    public async Task<List<Store>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Stores
            .Include(s => s.Owner)
            .ToListAsync(cancellationToken);
    }

    public async Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Stores
            .Include(s => s.Owner)
            .Include(s => s.Teams)
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<Store>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Stores
            .Include(s => s.Owner)
            .Where(s => s.OwnerUserId == ownerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Store> AddAsync(Store store, CancellationToken cancellationToken = default)
    {
        await _context.Stores.AddAsync(store, cancellationToken);
        return store;
    }

    public Task UpdateAsync(Store store, CancellationToken cancellationToken = default)
    {
        _context.Stores.Update(store);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var store = await _context.Stores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        
        if (store == null)
        {
            throw new KeyNotFoundException($"Store with ID {id} not found.");
        }

        if (store.IsDeleted)
        {
            throw new InvalidOperationException($"Store with ID {id} is already deleted.");
        }

        store.IsDeleted = true;
        store.DeletedAt = DateTime.UtcNow;
    }
}
