using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationPlatform.Application.Common.Models;
using ReservationPlatform.Application.Restaurants.Commands.ApproveRestaurant;
using ReservationPlatform.Application.Restaurants.Queries.GetRestaurants;

namespace ReservationPlatform.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController(IMediator mediator) : ControllerBase
{
    [HttpGet("restaurants")]
    public async Task<IActionResult> GetRestaurants(
        [FromQuery] bool? approved, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetRestaurantsQuery(null, null, null, null, null, page, pageSize, "name"), ct);
        return Ok(ApiResponse<PaginatedResult<Application.Restaurants.DTOs.RestaurantListDto>>.Ok(result));
    }

    [HttpPut("restaurants/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new ApproveRestaurantCommand(id, request.Approved), ct);
        return Ok(ApiResponse<Application.Restaurants.DTOs.RestaurantListDto>.Ok(result,
            request.Approved ? "Restaurant approved." : "Restaurant approval revoked."));
    }
}

public record ApproveRequest(bool Approved);
