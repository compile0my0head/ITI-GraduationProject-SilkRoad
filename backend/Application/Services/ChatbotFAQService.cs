using Application.Common.Interfaces;
using Application.DTOs.ChatbotFAQs;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class ChatbotFAQService : IChatbotFAQService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStoreContext _storeContext;

    public ChatbotFAQService(IUnitOfWork unitOfWork, IMapper mapper, IStoreContext storeContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storeContext = storeContext;
    }

    public async Task<List<ChatbotFAQDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // StoreId filtering is handled automatically by EF Core global query filters
        var faqs = await _unitOfWork.ChatbotFAQs.GetAllAsync(cancellationToken);
        return _mapper.Map<List<ChatbotFAQDto>>(faqs);
    }

    public async Task<ChatbotFAQDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var faq = await _unitOfWork.ChatbotFAQs.GetByIdAsync(Guid.Parse(id.ToString()), cancellationToken);
        return faq == null ? null : _mapper.Map<ChatbotFAQDto>(faq);
    }

    public async Task<List<ChatbotFAQDto>> GetByStoreIdAsync(int storeId, CancellationToken cancellationToken = default)
    {
        var faqs = await _unitOfWork.ChatbotFAQs.GetByStoreIdAsync(Guid.Parse(storeId.ToString()), cancellationToken);
        return _mapper.Map<List<ChatbotFAQDto>>(faqs);
    }

    public async Task<ChatbotFAQDto> CreateAsync(CreateChatbotFAQRequest request, CancellationToken cancellationToken = default)
    {
        // Get StoreId from StoreContext (set by middleware from X-Store-ID header)
        if (!_storeContext.HasStoreContext)
        {
            throw new InvalidOperationException("StoreId is required for creating a chatbot FAQ. Ensure X-Store-ID header is provided.");
        }

        var faq = _mapper.Map<ChatbotFAQ>(request);
        faq.StoreId = _storeContext.StoreId!.Value; // Auto-inject StoreId
        
        var createdFaq = await _unitOfWork.ChatbotFAQs.AddAsync(faq, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<ChatbotFAQDto>(createdFaq);
    }

    public async Task<ChatbotFAQDto> UpdateAsync(int id, UpdateChatbotFAQRequest request, CancellationToken cancellationToken = default)
    {
        var faq = await _unitOfWork.ChatbotFAQs.GetByIdAsync(Guid.Parse(id.ToString()), cancellationToken);
        
        if (faq == null)
        {
            throw new KeyNotFoundException($"ChatbotFAQ with ID {id} not found.");
        }

        _mapper.Map(request, faq);
        await _unitOfWork.ChatbotFAQs.UpdateAsync(faq, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<ChatbotFAQDto>(faq);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ChatbotFAQs.DeleteAsync(Guid.Parse(id.ToString()), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
