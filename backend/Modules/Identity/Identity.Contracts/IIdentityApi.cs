namespace Identity.Contracts;

public interface IIdentityApi
{
    Task<List<UserSummaryDto>> SearchByUsernameAsync(string term, int limit);

    Task<List<UserSummaryDto>> GetByIdsAsync(List<Guid> userIds);
}
