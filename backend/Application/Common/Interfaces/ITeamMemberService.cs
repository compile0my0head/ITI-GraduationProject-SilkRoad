using Application.DTOs.Teams;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface ITeamMemberService
{
    Task<TeamMemberDto> AddMemberAsync(Guid teamId, Guid userId, TeamRole role, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);
    Task<TeamMemberDto> UpdateMemberRoleAsync(Guid teamId, Guid userId, TeamRole role, CancellationToken cancellationToken = default);
    Task<List<TeamMemberDto>> GetTeamMembersAsync(Guid teamId, CancellationToken cancellationToken = default);
}
