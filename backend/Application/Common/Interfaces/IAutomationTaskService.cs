using Application.DTOs.AutomationTasks;

namespace Application.Common.Interfaces;

public interface IAutomationTaskService
{
    Task<List<AutomationTaskDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AutomationTaskDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<AutomationTaskDto>> GetByStoreIdAsync(int storeId, CancellationToken cancellationToken = default);
    Task<AutomationTaskDto> CreateAsync(CreateAutomationTaskRequest request, CancellationToken cancellationToken = default);
    Task<AutomationTaskDto> UpdateAsync(int id, UpdateAutomationTaskRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
