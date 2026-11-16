using Shared.Domain.Exceptions;

namespace Payment.Domain.Products;

public sealed record Price
{
    public int Value { get; private init; }

    private Price() { }

    public Price(int value)
    {
        if (value < 0)
        {
            throw new DomainException("A price must be zero or a positive whole number of EC.");
        }

        Value = value;
    }
}
