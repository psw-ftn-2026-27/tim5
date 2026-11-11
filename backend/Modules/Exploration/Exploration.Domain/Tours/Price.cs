using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record Price
{
    public bool IsFree { get; private init; }
    public int Amount { get; private init; }

    public Price(bool isFree, int amount)
    {
        if (isFree && amount != 0)
        {
            throw new DomainException("A free price cannot have an amount.");
        }
        if (!isFree && amount <= 0)
        {
            throw new DomainException("A paid price requires an amount greater than 0.");
        }

        IsFree = isFree;
        Amount = amount;
    }
}
