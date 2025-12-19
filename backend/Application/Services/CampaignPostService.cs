using Application.Common.Interfaces;
using Application.DTOs.CampaignPosts;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class CampaignPostService : ICampaignPostService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStoreContext _storeContext;

    public CampaignPostService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IStoreContext storeContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _storeContext = storeContext;
    }

    public async Task<List<CampaignPostDto>> GetAllPostsAsync(CancellationToken cancellationToken = default)
    {
        // All posts are filtered by store context automatically via EF Core query filters
        var posts = await _unitOfWork.CampaignPosts.GetAllAsync(cancellationToken);
        return _mapper.Map<List<CampaignPostDto>>(posts);
    }

    public async Task<CampaignPostDto?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _unitOfWork.CampaignPosts.GetByIdAsync(postId, cancellationToken);
        return post == null ? null : _mapper.Map<CampaignPostDto>(post);
    }

    public async Task<CampaignPostDto> CreatePostAsync(CreateCampaignPostRequest request, CancellationToken cancellationToken = default)
    {
        // Get StoreId from StoreContext
        if (!_storeContext.HasStoreContext)
        {
            throw new InvalidOperationException("StoreId is required for creating a campaign post. Ensure X-Store-ID header is provided.");
        }

        var storeId = _storeContext.StoreId!.Value;

        // Validate that campaign exists and belongs to the current store
        var campaign = await _unitOfWork.Campaigns.GetByIdAsync(request.CampaignId);
        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID {request.CampaignId} not found.");
        }

        if (campaign.StoreId != storeId)
        {
            throw new InvalidOperationException($"Campaign {request.CampaignId} does not belong to the current store.");
        }

        // Get all connected (active) social platforms for this store
        var connectedPlatforms = await _unitOfWork.SocialPlatforms.GetConnectedPlatformsByStoreIdAsync(storeId, cancellationToken);

        if (!connectedPlatforms.Any())
        {
            throw new InvalidOperationException(
                "No connected social platforms found for this store. " +
                "Please connect at least one social platform (Facebook, Instagram, etc.) before creating posts.");
        }

        // Create the CampaignPost entity
        var post = _mapper.Map<CampaignPost>(request);
        post.PublishStatus = PublishStatus.Pending.ToString();
        post.CreatedAt = DateTime.UtcNow;

        // Determine scheduled time (use post's ScheduledAt or current time for immediate publishing)
        var scheduledTime = request.ScheduledAt ?? DateTime.UtcNow;

        // ?? CRITICAL: Auto-generate CampaignPostPlatform for each connected platform
        // This ensures the post will be published to all connected platforms
        foreach (var platform in connectedPlatforms)
        {
            var platformPost = new CampaignPostPlatform
            {
                Id = Guid.NewGuid(),
                // CampaignPostId will be set by EF Core after SaveChanges
                PlatformId = platform.Id,
                ScheduledAt = scheduledTime,
                PublishStatus = PublishStatus.Pending.ToString(),
                PublishedAt = null,
                ExternalPostId = null,
                ErrorMessage = null
            };

            post.PlatformPosts.Add(platformPost);
        }

        // Save CampaignPost and all CampaignPostPlatform records in one transaction
        var createdPost = await _unitOfWork.CampaignPosts.AddAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties to return complete data
        var savedPost = await _unitOfWork.CampaignPosts.GetByIdAsync(createdPost.Id, cancellationToken);
        
        return _mapper.Map<CampaignPostDto>(savedPost);
    }

    public async Task<CampaignPostDto> UpdatePostAsync(Guid postId, UpdateCampaignPostRequest request, CancellationToken cancellationToken = default)
    {
        var post = await _unitOfWork.CampaignPosts.GetByIdAsync(postId, cancellationToken);

        if (post == null)
        {
            throw new KeyNotFoundException($"Campaign post with ID {postId} not found.");
        }

        // If CampaignId is being changed, validate the new campaign
        if (request.CampaignId.HasValue && request.CampaignId.Value != post.CampaignId)
        {
            if (!_storeContext.HasStoreContext)
            {
                throw new InvalidOperationException("StoreId is required for updating campaign post. Ensure X-Store-ID header is provided.");
            }

            var storeId = _storeContext.StoreId!.Value;
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(request.CampaignId.Value);
            
            if (campaign == null)
            {
                throw new KeyNotFoundException($"Campaign with ID {request.CampaignId.Value} not found.");
            }

            if (campaign.StoreId != storeId)
            {
                throw new InvalidOperationException($"Campaign {request.CampaignId.Value} does not belong to the current store.");
            }
        }

        // Store old scheduled time to check if it changed
        var oldScheduledAt = post.ScheduledAt;

        // Manually update properties to ensure they are applied
        if (request.CampaignId.HasValue)
        {
            post.CampaignId = request.CampaignId.Value;
        }
        
        if (!string.IsNullOrEmpty(request.PostCaption))
        {
            post.PostCaption = request.PostCaption;
        }
        
        if (request.PostImageUrl != null) // Allow empty string to clear the URL
        {
            post.PostImageUrl = request.PostImageUrl;
        }
        
        if (request.ScheduledAt.HasValue)
        {
            post.ScheduledAt = request.ScheduledAt.Value;
        }

        await _unitOfWork.CampaignPosts.UpdateAsync(post, cancellationToken);

        // ?? CRITICAL: Propagate updates to all CampaignPostPlatform records
        // Get all platform posts for this campaign post using optimized query
        var platformPosts = await _unitOfWork.CampaignPostPlatforms.GetByCampaignPostIdAsync(postId, cancellationToken);

        foreach (var platformPost in platformPosts)
        {
            // Only update if not already published (don't modify published posts)
            if (platformPost.PublishStatus == PublishStatus.Pending.ToString() || 
                platformPost.PublishStatus == PublishStatus.Failed.ToString())
            {
                // Update scheduled time if it changed in the parent post
                if (request.ScheduledAt.HasValue && request.ScheduledAt.Value != oldScheduledAt)
                {
                    platformPost.ScheduledAt = request.ScheduledAt.Value;
                    
                    // Reset status to Pending if it was Failed
                    if (platformPost.PublishStatus == PublishStatus.Failed.ToString())
                    {
                        platformPost.PublishStatus = PublishStatus.Pending.ToString();
                        platformPost.ErrorMessage = null;
                    }
                }

                await _unitOfWork.CampaignPostPlatforms.UpdateAsync(platformPost, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties to return complete data
        var updatedPost = await _unitOfWork.CampaignPosts.GetByIdAsync(postId, cancellationToken);

        return _mapper.Map<CampaignPostDto>(updatedPost!);
    }

    public async Task DeletePostAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.CampaignPosts.DeleteAsync(postId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
