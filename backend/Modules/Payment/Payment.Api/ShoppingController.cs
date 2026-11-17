using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Shopping;
using Shared.Api;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Payment.Api;

[ApiController]
[Route("api/payment")]
[Authorize]
public sealed class ShoppingController : ControllerBase
{
    private readonly ShoppingService _shoppingService;
    private readonly ShoppingQueries _shoppingQueries;

    public ShoppingController(ShoppingService shoppingService, ShoppingQueries shoppingQueries)
    {
        _shoppingService = shoppingService;
        _shoppingQueries = shoppingQueries;
    }

    [HttpGet("products/{entityId:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid entityId)
    {
        return await _shoppingQueries.GetProductAsync(entityId);
    }

    [HttpGet("cart")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult<ShoppingCartDto>> GetMyCart()
    {
        return await _shoppingQueries.GetMyCartAsync(User.GetUserId());
    }

    [HttpGet("cart/checkout-preview")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult<CheckoutPreviewDto>> GetCheckoutPreview(
        [FromQuery] string? couponCode,
        [FromQuery] Guid? giftCardId)
    {
        return await _shoppingQueries.GetCheckoutPreviewAsync(User.GetUserId(), couponCode, giftCardId);
    }

    [HttpPost("cart/items")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> AddToCart(AddToCartDto dto)
    {
        await _shoppingService.AddToCartAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpPost("cart/checkout")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult<Guid>> Checkout(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CheckoutBenefitsDto? benefits = null)
    {
        var purchaseId = await _shoppingService.CheckoutAsync(User.GetUserId(), benefits);
        return benefits is null ? NoContent() : purchaseId;
    }

    [HttpGet("purchases/{id:guid}")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult<PurchaseDto>> GetPurchase(Guid id)
    {
        return await _shoppingQueries.GetPurchaseAsync(id, User.GetUserId());
    }

    [HttpDelete("cart/items/{entityId:guid}")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> RemoveFromCart(Guid entityId)
    {
        await _shoppingService.RemoveFromCartAsync(User.GetUserId(), entityId);
        return NoContent();
    }
}
