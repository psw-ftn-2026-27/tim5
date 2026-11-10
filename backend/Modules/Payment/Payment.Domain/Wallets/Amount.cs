using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed record Amount
{
    public int Value { get; private init; }

    private Amount() { }

    public Amount(int value)
    {
        if (value <= 0)
        {
            throw new DomainException("An amount must be a positive whole number of EC.");
        }

        Value = value;
    }
}
