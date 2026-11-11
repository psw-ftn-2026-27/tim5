using Shared.Domain;
using Social.Application.ClubReading;

namespace Social.Application.Clubs;

public interface IClubReadRepository
{
    Task<ClubDto?> GetByIdAsync(Guid id);
    Task<PageResult<ClubDto>> GetAllAsync(int page, int pageSize);
}
