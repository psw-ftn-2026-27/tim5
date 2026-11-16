using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Shopping;
using Shared.Api;

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

    [HttpPost("cart/items")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> AddToCart(AddToCartDto dto)
    {
        await _shoppingService.AddToCartAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("cart/items/{entityId:guid}")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> RemoveFromCart(Guid entityId)
    {
        await _shoppingService.RemoveFromCartAsync(User.GetUserId(), entityId);
        return NoContent();
    }
}
