using Application.Common.Interfaces;
using Application.DTOs.ChatbotFAQs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// ChatbotFAQ API Controller
/// STORE-SCOPED endpoints - X-Store-ID required
/// </summary>
[ApiController]
[Route("api/chatbot-faq")]
[Produces("application/json")]
[Authorize]
public class ChatbotFAQController : ControllerBase
{
    private readonly IChatbotFAQService _chatbotFAQService;

    public ChatbotFAQController(IChatbotFAQService chatbotFAQService)
    {
        _chatbotFAQService = chatbotFAQService;
    }

    /// <summary>
    /// Get all chatbot FAQs
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllFAQs(CancellationToken cancellationToken)
    {
        var faqs = await _chatbotFAQService.GetAllAsync(cancellationToken);
        return Ok(faqs);
    }

    /// <summary>
    /// Get chatbot FAQ by ID
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet("{faqId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFAQById(int faqId, CancellationToken cancellationToken)
    {
        var faq = await _chatbotFAQService.GetByIdAsync(faqId, cancellationToken);
        
        if (faq == null)
            return NotFound(new { message = $"ChatbotFAQ with ID {faqId} not found" });
        
        return Ok(faq);
    }

    /// <summary>
    /// Create a new chatbot FAQ
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFAQ([FromBody] CreateChatbotFAQRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var faq = await _chatbotFAQService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetFAQById), new { faqId = faq.Id }, faq);
    }

    /// <summary>
    /// Update an existing chatbot FAQ
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPut("{faqId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateFAQ(int faqId, [FromBody] UpdateChatbotFAQRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var faq = await _chatbotFAQService.UpdateAsync(faqId, request, cancellationToken);
            return Ok(faq);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"ChatbotFAQ with ID {faqId} not found" });
        }
    }

    /// <summary>
    /// Delete a chatbot FAQ
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpDelete("{faqId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteFAQ(int faqId, CancellationToken cancellationToken)
    {
        try
        {
            await _chatbotFAQService.DeleteAsync(faqId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"ChatbotFAQ with ID {faqId} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
