using Identity.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Core;

internal sealed class IdentityApi : IIdentityApi
{
    private const int MaxLimit = 50;

    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityApi(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserSummaryDto>> SearchByUsernameAsync(string term, int limit)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return [];
        }

        var normalizedTerm = _userManager.NormalizeName(term.Trim());
        limit = Math.Clamp(limit, 1, MaxLimit);

        return await _userManager.Users
            .Where(user => user.NormalizedUserName!.Contains(normalizedTerm!))
            .OrderBy(user => user.UserName)
            .Take(limit)
            .Select(user => new UserSummaryDto(user.Id, user.UserName!))
            .ToListAsync();
    }

    public async Task<List<UserSummaryDto>> GetByIdsAsync(List<Guid> userIds)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        return await _userManager.Users
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new UserSummaryDto(user.Id, user.UserName!))
            .ToListAsync();
    }
}
