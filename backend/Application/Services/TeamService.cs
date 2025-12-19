using Application.Common.Interfaces;
using Application.DTOs.Teams;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class TeamService : ITeamService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStoreContext _storeContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStoreAuthorizationService _storeAuthorizationService;

    public TeamService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IStoreContext storeContext,
        ICurrentUserService currentUserService,
        IStoreAuthorizationService storeAuthorizationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storeContext = storeContext;
        _currentUserService = currentUserService;
        _storeAuthorizationService = storeAuthorizationService;
    }

    /// <summary>
    /// Get all teams user has access to (owned stores + team member)
    /// GLOBAL endpoint - works WITHOUT StoreId
    /// </summary>
    public async Task<List<TeamDto>> GetMyTeamsAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedAccessException("User must be authenticated to access teams.");
        }

        // Get all store IDs user has access to
        var userStoreIds = await _storeAuthorizationService.GetUserStoreIdsAsync(
            _currentUserService.UserId.Value, 
            cancellationToken);

        // Get all teams from those stores (bypassing store context filter)
        var allTeams = await _unitOfWork.Teams.GetAllAsync(cancellationToken);
        var userTeams = allTeams.Where(t => userStoreIds.Contains(t.StoreId)).ToList();
        
        return _mapper.Map<List<TeamDto>>(userTeams);
    }

    /// <summary>
    /// Get team by ID
    /// GLOBAL endpoint - works WITHOUT StoreId
    /// </summary>
    public async Task<TeamDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(id, cancellationToken);
        return team == null ? null : _mapper.Map<TeamDto>(team);
    }

    /// <summary>
    /// Get all teams in current store
    /// STORE-SCOPED endpoint - requires X-Store-ID header
    /// </summary>
    public async Task<List<TeamDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // StoreId filtering is handled automatically by EF Core global query filters
        var teams = await _unitOfWork.Teams.GetAllAsync(cancellationToken);
        return _mapper.Map<List<TeamDto>>(teams);
    }

    /// <summary>
    /// Create a team in current store
    /// STORE-SCOPED endpoint - requires X-Store-ID header
    /// </summary>
    public async Task<TeamDto> CreateAsync(CreateTeamRequest request, CancellationToken cancellationToken = default)
    {
        // Get StoreId from StoreContext (set by middleware from X-Store-ID header)
        if (!_storeContext.HasStoreContext)
        {
            throw new InvalidOperationException("StoreId is required for creating a team. Ensure X-Store-ID header is provided.");
        }

        var team = _mapper.Map<Team>(request);
        team.StoreId = _storeContext.StoreId!.Value; // Auto-inject StoreId
        
        var createdTeam = await _unitOfWork.Teams.AddAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<TeamDto>(createdTeam);
    }

    /// <summary>
    /// Update a team
    /// STORE-SCOPED endpoint - requires X-Store-ID header
    /// </summary>
    public async Task<TeamDto> UpdateAsync(Guid id, UpdateTeamRequest request, CancellationToken cancellationToken = default)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(id, cancellationToken);
        
        if (team == null)
        {
            throw new KeyNotFoundException($"Team with ID {id} not found.");
        }

        _mapper.Map(request, team);
        await _unitOfWork.Teams.UpdateAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<TeamDto>(team);
    }

    /// <summary>
    /// Delete a team
    /// STORE-SCOPED endpoint - requires X-Store-ID header
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Teams.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
