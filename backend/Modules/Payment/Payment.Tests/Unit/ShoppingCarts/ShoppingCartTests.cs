using FluentAssertions;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.ShoppingCarts;

public class ShoppingCartTests
{
    private static readonly Guid Author = Guid.Parse("00000000-0000-0000-0006-000000000001");
    private static readonly Guid FirstTour = Guid.Parse("00000000-0000-0000-0006-000000000011");
    private static readonly Guid SecondTour = Guid.Parse("00000000-0000-0000-0006-000000000012");

    private static ShoppingCart CreateCartWithOneTour()
    {
        var cart = new ShoppingCart(WellKnownUsers.Explorer);
        cart.AddItem(FirstTour, "Petrovaradinska tvrdjava", new Price(150), Author);
        return cart;
    }

    [Fact]
    public void A_free_tour_is_rejected_and_the_cart_stays_the_same()
    {
        var cart = CreateCartWithOneTour();

        var add = () => cart.AddItem(SecondTour, "Dunavski kej", new Price(0), Author);

        add.Should().Throw<DomainException>();
        cart.Items.Should().ContainSingle(item => item.EntityId == FirstTour);
    }

    [Fact]
    public void A_tour_already_in_the_cart_is_rejected_and_the_cart_stays_the_same()
    {
        var cart = CreateCartWithOneTour();

        var add = () => cart.AddItem(FirstTour, "Petrovaradinska tvrdjava", new Price(150), Author);

        add.Should().Throw<DomainException>();
        cart.Items.Should().ContainSingle(item => item.EntityId == FirstTour);
    }

    [Fact]
    public void A_product_owned_by_the_tourist_of_the_cart_is_rejected_and_the_cart_stays_the_same()
    {
        var cart = CreateCartWithOneTour();

        var add = () => cart.AddItem(SecondTour, "Novosadska sinagoga", new Price(200), WellKnownUsers.Explorer);

        add.Should().Throw<DomainException>();
        cart.Items.Should().ContainSingle(item => item.EntityId == FirstTour);
    }

    [Fact]
    public void Total_of_a_cart_with_two_tours_is_the_sum_of_their_prices()
    {
        var cart = CreateCartWithOneTour();

        cart.AddItem(SecondTour, "Novosadska sinagoga", new Price(200), Author);

        cart.Total.Should().Be(350);
    }

    [Fact]
    public void Removing_a_tour_from_a_cart_with_two_tours_removes_only_that_tour()
    {
        var cart = CreateCartWithOneTour();
        cart.AddItem(SecondTour, "Novosadska sinagoga", new Price(200), Author);

        cart.RemoveItem(FirstTour);

        cart.Items.Should().ContainSingle(item => item.EntityId == SecondTour);
    }

    [Fact]
    public void Removing_a_tour_that_is_not_in_the_cart_is_rejected_as_not_existing()
    {
        var cart = CreateCartWithOneTour();

        var remove = () => cart.RemoveItem(SecondTour);

        remove.Should().Throw<NotFoundException>();
        cart.Items.Should().ContainSingle(item => item.EntityId == FirstTour);
    }
}
