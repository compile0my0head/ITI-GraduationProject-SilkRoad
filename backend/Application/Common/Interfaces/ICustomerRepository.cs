using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Customer>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByPSIDAsync(string psid, Guid storeId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByPhoneAsync(string phone, Guid storeId, CancellationToken cancellationToken = default);
    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
