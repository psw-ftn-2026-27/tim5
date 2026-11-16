using FluentAssertions;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit.Products;

public class PriceTests
{
    [Fact]
    public void Zero_EC_is_accepted_as_the_price_of_a_free_tour()
    {
        var price = new Price(0);

        price.Value.Should().Be(0);
    }

    [Fact]
    public void A_negative_price_is_rejected()
    {
        var create = () => new Price(-1);

        create.Should().Throw<DomainException>();
    }
}
