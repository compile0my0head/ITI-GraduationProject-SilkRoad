using Application.DTOs.ChatbotFAQs;

namespace Application.Common.Interfaces;

public interface IChatbotFAQService
{
    Task<List<ChatbotFAQDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ChatbotFAQDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<ChatbotFAQDto>> GetByStoreIdAsync(int storeId, CancellationToken cancellationToken = default);
    Task<ChatbotFAQDto> CreateAsync(CreateChatbotFAQRequest request, CancellationToken cancellationToken = default);
    Task<ChatbotFAQDto> UpdateAsync(int id, UpdateChatbotFAQRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
