using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IAutomationTaskRepository
{
    Task<List<AutomationTask>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AutomationTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<AutomationTask>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<AutomationTask> AddAsync(AutomationTask automationTask, CancellationToken cancellationToken = default);
    Task UpdateAsync(AutomationTask automationTask, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
