using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ChatbotFAQRepository : IChatbotFAQRepository
{
    private readonly SaasDbContext _context;

    public ChatbotFAQRepository(SaasDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChatbotFAQ>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Simplified query - no includes for GetAll to prevent issues with empty collections
        return await _context.ChatbotFAQs
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatbotFAQ?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Include related entities only for single item retrieval
        return await _context.ChatbotFAQs
            .Include(cf => cf.Store)
            .FirstOrDefaultAsync(cf => cf.Id == id, cancellationToken);
    }

    public async Task<List<ChatbotFAQ>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        return await _context.ChatbotFAQs
            .Where(cf => cf.StoreId == storeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatbotFAQ> AddAsync(ChatbotFAQ chatbotFAQ, CancellationToken cancellationToken = default)
    {
        await _context.ChatbotFAQs.AddAsync(chatbotFAQ, cancellationToken);
        return chatbotFAQ;
    }

    public Task UpdateAsync(ChatbotFAQ chatbotFAQ, CancellationToken cancellationToken = default)
    {
        _context.ChatbotFAQs.Update(chatbotFAQ);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var chatbotFAQ = await _context.ChatbotFAQs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(cf => cf.Id == id, cancellationToken);
        
        if (chatbotFAQ == null)
        {
            throw new KeyNotFoundException($"ChatbotFAQ with ID {id} not found.");
        }

        if (chatbotFAQ.IsDeleted)
        {
            throw new InvalidOperationException($"ChatbotFAQ with ID {id} is already deleted.");
        }

        chatbotFAQ.IsDeleted = true;
        chatbotFAQ.DeletedAt = DateTime.UtcNow;
    }
}
