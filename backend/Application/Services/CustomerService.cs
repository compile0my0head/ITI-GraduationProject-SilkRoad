using Application.Common.Interfaces;
using Application.DTOs.Customers;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStoreContext _storeContext;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, IStoreContext storeContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storeContext = storeContext;
    }

    public async Task<List<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // StoreId filtering is handled automatically by EF Core global query filters
        var customers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);
        return _mapper.Map<List<CustomerDto>>(customers);
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        return customer == null ? null : _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        // Get StoreId from StoreContext (set by middleware from X-Store-ID header)
        if (!_storeContext.HasStoreContext)
        {
            throw new InvalidOperationException("StoreId is required for creating a customer. Ensure X-Store-ID header is provided.");
        }

        var customer = _mapper.Map<Customer>(request);
        customer.StoreId = _storeContext.StoreId!.Value; // Auto-inject StoreId
        
        var createdCustomer = await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<CustomerDto>(createdCustomer);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {id} not found.");
        }

        _mapper.Map(request, customer);
        await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Customers.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
