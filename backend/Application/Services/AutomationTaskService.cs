using Application.Common.Interfaces;
using Application.DTOs.AutomationTasks;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class AutomationTaskService : IAutomationTaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStoreContext _storeContext;

    public AutomationTaskService(IUnitOfWork unitOfWork, IMapper mapper, IStoreContext storeContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storeContext = storeContext;
    }

    public async Task<List<AutomationTaskDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // StoreId filtering is handled automatically by EF Core global query filters
        var tasks = await _unitOfWork.AutomationTasks.GetAllAsync(cancellationToken);
        return _mapper.Map<List<AutomationTaskDto>>(tasks);
    }

    public async Task<AutomationTaskDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.AutomationTasks.GetByIdAsync(Guid.Parse(id.ToString()), cancellationToken);
        return task == null ? null : _mapper.Map<AutomationTaskDto>(task);
    }

    public async Task<List<AutomationTaskDto>> GetByStoreIdAsync(int storeId, CancellationToken cancellationToken = default)
    {
        var tasks = await _unitOfWork.AutomationTasks.GetByStoreIdAsync(Guid.Parse(storeId.ToString()), cancellationToken);
        return _mapper.Map<List<AutomationTaskDto>>(tasks);
    }

    public async Task<AutomationTaskDto> CreateAsync(CreateAutomationTaskRequest request, CancellationToken cancellationToken = default)
    {
        // Get StoreId from StoreContext (set by middleware from X-Store-ID header)
        if (!_storeContext.HasStoreContext)
        {
            throw new InvalidOperationException("StoreId is required for creating an automation task. Ensure X-Store-ID header is provided.");
        }

        var task = _mapper.Map<AutomationTask>(request);
        task.StoreId = _storeContext.StoreId!.Value; // Auto-inject StoreId
        
        var createdTask = await _unitOfWork.AutomationTasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<AutomationTaskDto>(createdTask);
    }

    public async Task<AutomationTaskDto> UpdateAsync(int id, UpdateAutomationTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.AutomationTasks.GetByIdAsync(Guid.Parse(id.ToString()), cancellationToken);
        
        if (task == null)
        {
            throw new KeyNotFoundException($"AutomationTask with ID {id} not found.");
        }

        _mapper.Map(request, task);
        await _unitOfWork.AutomationTasks.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<AutomationTaskDto>(task);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.AutomationTasks.DeleteAsync(Guid.Parse(id.ToString()), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
