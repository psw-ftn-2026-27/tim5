using FluentAssertions;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit.Products;

public class ProductTests
{
    [Fact]
    public void The_owner_can_confirm_ownership_of_their_product()
    {
        var ownerId = Guid.NewGuid();
        var product = new Product(Guid.NewGuid(), "Tour", ownerId, new Price(100));

        var ensureOwnership = () => product.EnsureOwnedBy(ownerId);

        ensureOwnership.Should().NotThrow();
    }

    [Fact]
    public void A_different_user_cannot_confirm_ownership_of_the_product()
    {
        var product = new Product(Guid.NewGuid(), "Tour", Guid.NewGuid(), new Price(100));

        var ensureOwnership = () => product.EnsureOwnedBy(Guid.NewGuid());

        ensureOwnership.Should().Throw<DomainException>();
    }
}
