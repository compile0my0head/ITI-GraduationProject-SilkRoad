using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AutomationTaskRepository : IAutomationTaskRepository
{
    private readonly SaasDbContext _context;

    public AutomationTaskRepository(SaasDbContext context)
    {
        _context = context;
    }

    public async Task<List<AutomationTask>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Simplified query - no includes for GetAll to prevent issues with empty collections
        return await _context.AutomationTasks
            .ToListAsync(cancellationToken);
    }

    public async Task<AutomationTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Include related entities only for single item retrieval
        return await _context.AutomationTasks
            .Include(at => at.Store)
            .Include(at => at.RelatedCampaignPost)
            .FirstOrDefaultAsync(at => at.Id == id, cancellationToken);
    }

    public async Task<List<AutomationTask>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        return await _context.AutomationTasks
            .Where(at => at.StoreId == storeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<AutomationTask> AddAsync(AutomationTask automationTask, CancellationToken cancellationToken = default)
    {
        await _context.AutomationTasks.AddAsync(automationTask, cancellationToken);
        return automationTask;
    }

    public Task UpdateAsync(AutomationTask automationTask, CancellationToken cancellationToken = default)
    {
        _context.AutomationTasks.Update(automationTask);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var automationTask = await _context.AutomationTasks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(at => at.Id == id, cancellationToken);
        
        if (automationTask == null)
        {
            throw new KeyNotFoundException($"AutomationTask with ID {id} not found.");
        }

        if (automationTask.IsDeleted)
        {
            throw new InvalidOperationException($"AutomationTask with ID {id} is already deleted.");
        }

        automationTask.IsDeleted = true;
        automationTask.DeletedAt = DateTime.UtcNow;
    }
}
