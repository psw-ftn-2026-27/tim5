using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class PriceTests
{
    [Fact]
    public void Free_price_cannot_have_an_amount()
    {
        var creation = () => new Price(true, 500);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Paid_price_requires_an_amount_greater_than_zero(int amount)
    {
        var creation = () => new Price(false, amount);

        creation.Should().Throw<DomainException>();
    }
}
