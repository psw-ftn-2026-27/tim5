using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Listing;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/products")]
[Authorize(Roles = "explorer")]
public sealed class ListingController : ControllerBase
{
    private readonly ListingQueries _listingQueries;

    public ListingController(ListingQueries listingQueries)
    {
        _listingQueries = listingQueries;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<MyProductDto>>> GetMyProducts()
    {
        return Ok(await _listingQueries.GetMyProductsAsync(User.GetUserId()));
    }
}
