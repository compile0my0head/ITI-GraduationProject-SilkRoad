using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IChatbotFAQRepository
{
    Task<List<ChatbotFAQ>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ChatbotFAQ?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ChatbotFAQ>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<ChatbotFAQ> AddAsync(ChatbotFAQ chatbotFAQ, CancellationToken cancellationToken = default);
    Task UpdateAsync(ChatbotFAQ chatbotFAQ, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
