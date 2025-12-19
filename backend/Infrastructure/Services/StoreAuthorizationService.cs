using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class StoreAuthorizationService : IStoreAuthorizationService
{
    private readonly IUnitOfWork _unitOfWork;

    public StoreAuthorizationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> UserBelongsToStoreAsync(Guid storeId, Guid userId, CancellationToken cancellationToken = default)
    {
        // First check if store exists at all
        var store = await _unitOfWork.Stores.GetByIdAsync(storeId, cancellationToken);
        if (store == null)
        {
            // Store doesn't exist - throw exception to be caught by middleware
            throw new KeyNotFoundException($"Store with ID {storeId} not found.");
        }

        // Check if user is the store owner
        if (store.OwnerUserId == userId)
        {
            return true;
        }

        // Check if user is a team member in any team of this store
        var teams = await _unitOfWork.Teams.GetByStoreIdAsync(storeId, cancellationToken);
        foreach (var team in teams)
        {
            var teamMembers = await _unitOfWork.Teams.GetTeamMembersAsync(team.Id, cancellationToken);
            if (teamMembers.Any(tm => tm.UserId == userId))
            {
                return true;
            }
        }

        // User doesn't have access to this store
        return false;
    }

    public async Task ValidateStoreAccessAsync(Guid storeId, Guid userId, CancellationToken cancellationToken = default)
    {
        var hasAccess = await UserBelongsToStoreAsync(storeId, userId, cancellationToken);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException($"User does not have access to store {storeId}");
        }
    }

    public async Task<List<Guid>> GetUserStoreIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var storeIds = new HashSet<Guid>();

        // Get owned stores
        var ownedStores = await _unitOfWork.Stores.GetByOwnerIdAsync(userId, cancellationToken);
        foreach (var store in ownedStores)
        {
            storeIds.Add(store.Id);
        }

        // Get stores where user is a team member
        var allTeams = await _unitOfWork.Teams.GetAllAsync(cancellationToken);
        foreach (var team in allTeams)
        {
            var teamMembers = await _unitOfWork.Teams.GetTeamMembersAsync(team.Id, cancellationToken);
            if (teamMembers.Any(tm => tm.UserId == userId))
            {
                storeIds.Add(team.StoreId);
            }
        }

        return storeIds.ToList();
    }
}
