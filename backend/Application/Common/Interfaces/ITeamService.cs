using Application.DTOs.Teams;

namespace Application.Common.Interfaces;

public interface ITeamService
{
    // GLOBAL endpoints - NO X-Store-ID required
    Task<List<TeamDto>> GetMyTeamsAsync(CancellationToken cancellationToken = default);
    Task<TeamDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    // STORE-SCOPED endpoints - X-Store-ID required
    Task<List<TeamDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TeamDto> CreateAsync(CreateTeamRequest request, CancellationToken cancellationToken = default);
    Task<TeamDto> UpdateAsync(Guid id, UpdateTeamRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Removed GetByStoreIdAsync - not needed, filtering handled by query filters
}
