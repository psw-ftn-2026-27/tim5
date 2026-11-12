using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class ClubNameUniqueness
{
    public void CheckForNewClub(ClubName name, Club? clubWithName)
    {
        if (clubWithName is not null && clubWithName.Name.NormalizedValue == name.NormalizedValue)
        {
            throw new DomainException($"The club name '{name.Value}' is already taken.");
        }
    }

    public void CheckForRename(Club club, ClubName name, Club? clubWithName)
    {
        if (clubWithName is not null && clubWithName.Id != club.Id && clubWithName.Name.NormalizedValue == name.NormalizedValue)
        {
            throw new DomainException($"The club name '{name.Value}' is already taken.");
        }
    }
}
