using Application.Common.Interfaces;
using Application.DTOs.SocialPlatforms;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class SocialPlatformService : ISocialPlatformService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStoreContext _storeContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private IGenericRepository<SocialPlatform> repo => _unitOfWork.GetRepository<SocialPlatform>();

    public SocialPlatformService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ICurrentUserService currentUserService,
        IStoreContext storeContext,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _storeContext = storeContext;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<List<SocialPlatformDto>> GetPlatformsByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        // StoreId filtering is handled automatically by EF Core global query filters
        var platforms = await repo.FindAsync(
            p => p.StoreId == storeId,
            p => p.Store
        );

        return _mapper.Map<List<SocialPlatformDto>>(platforms);
    }

    public async Task<SocialPlatformDto?> GetPlatformByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // StoreId filtering is handled automatically by EF Core global query filters
        var platform = await repo.GetByIdAsync(id, p => p.Store);

        if (platform == null)
        {
            return null;
        }

        return _mapper.Map<SocialPlatformDto>(platform);
    }

    public async Task<SocialPlatformDto> CreatePlatformAsync(CreateSocialPlatformRequest request, CancellationToken cancellationToken = default)
    {
        // Get StoreId from StoreContext (set by middleware from X-Store-ID header)
        if (!_storeContext.HasStoreContext)
        {
            throw new InvalidOperationException("Store ID is required. Please provide a valid X-Store-ID header.");
        }

        var storeId = _storeContext.StoreId!.Value;

        // Parse the platform name BEFORE the query (cannot be done inside LINQ to SQL)
        if (!Enum.TryParse<PlatformName>(request.PlatformName, true, out var platformName))
        {
            throw new ArgumentException($"Invalid platform name: '{request.PlatformName}'. Valid values are: {string.Join(", ", Enum.GetNames<PlatformName>())}");
        }

        // Check if platform already exists for this store
        var existingPlatforms = await repo.FindAsync(
            p => p.StoreId == storeId && p.PlatformName == platformName);

        if (existingPlatforms.Any())
        {
            throw new InvalidOperationException($"{request.PlatformName} is already connected to this store. Please disconnect the existing connection first.");
        }

        // Map request to entity
        var platform = _mapper.Map<SocialPlatform>(request);
        platform.StoreId = storeId; // Auto-inject StoreId
        platform.CreatedAt = DateTime.UtcNow;
        platform.UpdatedAt = DateTime.UtcNow;

        await repo.AddAsync(platform);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SocialPlatformDto>(platform);
    }

    public async Task<SocialPlatformDto> UpdatePlatformAsync(Guid id, UpdateSocialPlatformRequest request, CancellationToken cancellationToken = default)
    {
        // Get existing platform (filtered by StoreId automatically)
        var platform = await repo.GetByIdAsync(id, p => p.Store);

        if (platform == null)
        {
            throw new KeyNotFoundException($"Platform with ID {id} not found.");
        }

        // Map updates
        _mapper.Map(request, platform);
        platform.UpdatedAt = DateTime.UtcNow;

        repo.Update(platform);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SocialPlatformDto>(platform);
    }

    public async Task<SocialPlatformDto> DisconnectPlatformAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Get existing platform (filtered by StoreId automatically)
        var platform = await repo.GetByIdAsync(id, p => p.Store);

        if (platform == null)
        {
            throw new KeyNotFoundException($"Platform with ID {id} not found.");
        }

        platform.IsConnected = false;
        platform.AccessToken = string.Empty;
        platform.UpdatedAt = DateTime.UtcNow;

        repo.Update(platform);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SocialPlatformDto>(platform);
    }

    public async Task DeletePlatformAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Get existing platform (filtered by StoreId automatically)
        var platform = await repo.GetByIdAsync(id, p => p.Store);

        if (platform == null)
        {
            throw new KeyNotFoundException($"Platform with ID {id} not found.");
        }

        // Soft delete
        repo.Delete(platform);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ===== OAuth Integration Methods =====

    public async Task<SocialPlatformDto> ConnectFacebookAsync(ConnectFacebookRequest request, CancellationToken cancellationToken = default)
    {
        // Get StoreId from StoreContext (set by middleware from X-Store-ID header)
        if (!_storeContext.HasStoreContext)
        {
            throw new InvalidOperationException("StoreId is required for connecting Facebook. Ensure X-Store-ID header is provided.");
        }

        var storeId = _storeContext.StoreId!.Value;

        // Get Facebook App credentials from configuration
        var appId = _configuration["Facebook:AppId"];
        var appSecret = _configuration["Facebook:AppSecret"];
        var redirectUri = request.RedirectUri ?? _configuration["Facebook:RedirectUri"];

        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
        {
            throw new InvalidOperationException("Facebook OAuth credentials are not configured.");
        }

        // Exchange authorization code for access token
        var tokenResponse = await ExchangeFacebookCodeForTokenAsync(request.Code, appId, appSecret, redirectUri!);

        // Get page information from Facebook
        var pageInfo = await GetFacebookPageInfoAsync(tokenResponse.AccessToken);

        // Check if platform already exists
        var existingPlatforms = await repo.FindAsync(
            p => p.StoreId == storeId && p.PlatformName == PlatformName.Facebook);

        SocialPlatform platform;

        if (existingPlatforms.Any())
        {
            // Update existing platform
            platform = existingPlatforms.First();
            platform.ExternalPageID = pageInfo.Id;
            platform.PageName = pageInfo.Name;
            platform.AccessToken = tokenResponse.AccessToken;
            platform.IsConnected = true;
            platform.UpdatedAt = DateTime.UtcNow;

            repo.Update(platform);
        }
        else
        {
            // Create new platform
            platform = new SocialPlatform
            {
                StoreId = storeId, // Auto-inject from context
                PlatformName = PlatformName.Facebook,
                ExternalPageID = pageInfo.Id,
                PageName = pageInfo.Name,
                AccessToken = tokenResponse.AccessToken,
                IsConnected = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(platform);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SocialPlatformDto>(platform);
    }

    public async Task<SocialPlatformDto> ConnectInstagramAsync(ConnectInstagramRequest request, CancellationToken cancellationToken = default)
    {
        // Get StoreId from StoreContext (set by middleware from X-Store-ID header)
        if (!_storeContext.HasStoreContext)
        {
            throw new InvalidOperationException("StoreId is required for connecting Instagram. Ensure X-Store-ID header is provided.");
        }

        // TODO: Implement Instagram OAuth flow
        throw new NotImplementedException("Instagram integration is not yet implemented.");
    }

    public async Task<SocialPlatformDto> RefreshPlatformTokenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Validate user is authenticated
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedAccessException("User must be authenticated to refresh platform token.");
        }

        // Get existing platform
        var platform = await repo.GetByIdAsync(id, p => p.Store);

        if (platform == null)
        {
            throw new KeyNotFoundException($"Platform with ID {id} not found.");
        }

        // Validate user owns the store
        if (platform.Store?.OwnerUserId != _currentUserService.UserId.Value)
        {
            throw new UnauthorizedAccessException("You can only refresh tokens for platforms in stores you own.");
        }

        // TODO: Implement token refresh logic based on platform type
        throw new NotImplementedException("Token refresh is not yet implemented.");
    }

    // ===== Private Helper Methods =====

    private async Task ValidateStoreOwnershipAsync(Guid storeId)
    {
        var storeRepo = _unitOfWork.GetRepository<Store>();
        var store = await storeRepo.GetByIdAsync(storeId);

        if (store == null)
        {
            throw new KeyNotFoundException($"Store with ID {storeId} not found.");
        }

        var currentUserId = _currentUserService.UserId!.Value;

        if (store.OwnerUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only manage social platforms for stores you own.");
        }
    }

    private async Task<FacebookTokenResponse> ExchangeFacebookCodeForTokenAsync(string code, string appId, string appSecret, string redirectUri)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var tokenUrl = $"https://graph.facebook.com/v18.0/oauth/access_token?" +
                       $"client_id={appId}" +
                       $"&client_secret={appSecret}" +
                       $"&code={code}" +
                       $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";

        var response = await httpClient.GetAsync(tokenUrl);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<FacebookTokenResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return tokenResponse ?? throw new InvalidOperationException("Failed to deserialize Facebook token response.");
    }

    private async Task<FacebookPageInfo> GetFacebookPageInfoAsync(string accessToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var pageUrl = $"https://graph.facebook.com/v18.0/me?fields=id,name&access_token={accessToken}";

        var response = await httpClient.GetAsync(pageUrl);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var pageInfo = JsonSerializer.Deserialize<FacebookPageInfo>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return pageInfo ?? throw new InvalidOperationException("Failed to deserialize Facebook page info.");
    }

    // ===== Helper Classes =====

    private class FacebookTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }

    private class FacebookPageInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
