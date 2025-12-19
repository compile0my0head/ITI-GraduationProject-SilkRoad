using Application.Common.Interfaces;
using Application.DTOs.Teams;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class TeamMemberService : ITeamMemberService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TeamMemberService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TeamMemberDto> AddMemberAsync(Guid teamId, Guid userId, TeamRole role, CancellationToken cancellationToken = default)
    {
        // Check if team exists
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            throw new KeyNotFoundException($"Team with ID {teamId} not found.");
        }

        // Check if user exists
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        // Check if member already exists
        var existingMember = await _unitOfWork.Teams.GetTeamMemberAsync(teamId, userId, cancellationToken);
        if (existingMember != null)
        {
            throw new InvalidOperationException($"User {userId} is already a member of team {teamId}.");
        }

        var member = new TeamMember
        {
            TeamId = teamId,
            UserId = userId,
            Role = role,
            AddedAt = DateTime.UtcNow
        };

        await _unitOfWork.Teams.AddMemberAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var addedMember = await _unitOfWork.Teams.GetTeamMemberAsync(teamId, userId, cancellationToken);
        return _mapper.Map<TeamMemberDto>(addedMember);
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _unitOfWork.Teams.GetTeamMemberAsync(teamId, userId, cancellationToken);
        if (member == null)
        {
            throw new KeyNotFoundException($"Team member not found.");
        }

        await _unitOfWork.Teams.RemoveMemberAsync(teamId, userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<TeamMemberDto> UpdateMemberRoleAsync(Guid teamId, Guid userId, TeamRole role, CancellationToken cancellationToken = default)
    {
        var member = await _unitOfWork.Teams.GetTeamMemberAsync(teamId, userId, cancellationToken);
        if (member == null)
        {
            throw new KeyNotFoundException($"Team member not found.");
        }

        member.Role = role;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TeamMemberDto>(member);
    }

    public async Task<List<TeamMemberDto>> GetTeamMembersAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var members = await _unitOfWork.Teams.GetTeamMembersAsync(teamId, cancellationToken);
        return _mapper.Map<List<TeamMemberDto>>(members);
    }
}
