using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.CouponCreating;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/coupons")]
[Authorize(Roles = "explorer")]
public sealed class CouponCreatingController : ControllerBase
{
    private readonly CouponCreatingService _couponCreatingService;
    private readonly CouponCreatingQueries _couponCreatingQueries;

    public CouponCreatingController(CouponCreatingService couponCreatingService, CouponCreatingQueries couponCreatingQueries)
    {
        _couponCreatingService = couponCreatingService;
        _couponCreatingQueries = couponCreatingQueries;
    }

    [HttpPost]
    public async Task<ActionResult<CreatedCouponDto>> CreateCoupon(CreateCouponDto dto)
    {
        var created = await _couponCreatingService.CreateCouponAsync(User.GetUserId(), dto);
        return CreatedAtAction(nameof(GetCoupon), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CouponDto>> GetCoupon(Guid id)
    {
        return Ok(await _couponCreatingQueries.GetCouponAsync(id, User.GetUserId()));
    }
}
