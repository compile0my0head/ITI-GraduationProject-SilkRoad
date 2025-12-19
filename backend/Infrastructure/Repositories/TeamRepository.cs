using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly SaasDbContext _context;

    public TeamRepository(SaasDbContext context)
    {
        _context = context;
    }

    public async Task<List<Team>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Store)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .ToListAsync(cancellationToken);
    }

    public async Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Store)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<Team>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Store)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Where(t => t.StoreId == storeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Team> AddAsync(Team team, CancellationToken cancellationToken = default)
    {
        await _context.Teams.AddAsync(team, cancellationToken);
        return team;
    }

    public Task UpdateAsync(Team team, CancellationToken cancellationToken = default)
    {
        _context.Teams.Update(team);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        
        if (team == null)
        {
            throw new KeyNotFoundException($"Team with ID {id} not found.");
        }

        if (team.IsDeleted)
        {
            throw new InvalidOperationException($"Team with ID {id} is already deleted.");
        }

        team.IsDeleted = true;
        team.DeletedAt = DateTime.UtcNow;
    }

    public async Task<TeamMember?> GetTeamMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .Include(tm => tm.User)
            .Include(tm => tm.Team)
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId, cancellationToken);
    }

    public async Task<List<TeamMember>> GetTeamMembersAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .Include(tm => tm.User)
            .Where(tm => tm.TeamId == teamId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddMemberAsync(TeamMember member, CancellationToken cancellationToken = default)
    {
        await _context.TeamMembers.AddAsync(member, cancellationToken);
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _context.TeamMembers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId, cancellationToken);
        
        if (member == null)
        {
            throw new KeyNotFoundException($"Team member not found for TeamId {teamId} and UserId {userId}.");
        }

        if (member.IsDeleted)
        {
            throw new InvalidOperationException($"Team member for TeamId {teamId} and UserId {userId} is already deleted.");
        }

        member.IsDeleted = true;
        member.DeletedAt = DateTime.UtcNow;
    }
}
