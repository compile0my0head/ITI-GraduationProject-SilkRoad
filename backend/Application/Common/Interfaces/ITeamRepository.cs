using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ITeamRepository
{
    Task<List<Team>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Team>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<Team> AddAsync(Team team, CancellationToken cancellationToken = default);
    Task UpdateAsync(Team team, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TeamMember?> GetTeamMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<TeamMember>> GetTeamMembersAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task AddMemberAsync(TeamMember member, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);
}
