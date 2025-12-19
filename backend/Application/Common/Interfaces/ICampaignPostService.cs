using Application.DTOs.CampaignPosts;

namespace Application.Common.Interfaces;

public interface ICampaignPostService
{
    Task<List<CampaignPostDto>> GetAllPostsAsync(CancellationToken cancellationToken = default);
    
    Task<CampaignPostDto?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default);
    
    Task<CampaignPostDto> CreatePostAsync(CreateCampaignPostRequest request, CancellationToken cancellationToken = default);
    
    Task<CampaignPostDto> UpdatePostAsync(Guid postId, UpdateCampaignPostRequest request, CancellationToken cancellationToken = default);
    
    Task DeletePostAsync(Guid postId, CancellationToken cancellationToken = default);
}
