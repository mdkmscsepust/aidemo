using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationPlatform.Application.Common.Models;
using ReservationPlatform.Application.OpeningHours.Commands.UpsertOpeningHours;
using ReservationPlatform.Application.OpeningHours.DTOs;
using ReservationPlatform.Application.Reservations.Queries.GetMyReservations;
using ReservationPlatform.Application.Restaurants.Queries.GetRestaurants;
using ReservationPlatform.Domain.Enums;

namespace ReservationPlatform.API.Controllers;

[ApiController]
[Route("api/owner")]
[Authorize(Policy = "OwnerOrAdmin")]
public class OwnerController(IMediator mediator) : ControllerBase
{
    [HttpGet("restaurants")]
    public async Task<IActionResult> GetMyRestaurants(
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        // For owner dashboard, we return all their restaurants including unapproved
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(
            new GetRestaurantsQuery(search, null, null, null, null, page, pageSize, "name"), ct);
        // Note: In a full implementation, GetRestaurantsQuery would have an ownerId filter.
        // For now, this returns all approved restaurants. The owner dashboard filters client-side.
        return Ok(ApiResponse<PaginatedResult<Application.Restaurants.DTOs.RestaurantListDto>>.Ok(result));
    }

    [HttpGet("reservations")]
    public async Task<IActionResult> GetReservations(
        [FromQuery] Guid? restaurantId,
        [FromQuery] string? date,
        [FromQuery] ReservationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        // Owner views reservations for their restaurants
        // For the dashboard we reuse GetMyReservations but scoped to restaurant owner
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        // This would require an owner-specific query in full implementation
        // Returning empty paginated result as placeholder; actual impl filters by ownerId
        return Ok(ApiResponse<PaginatedResult<Application.Reservations.DTOs.ReservationDto>>.Ok(
            PaginatedResult<Application.Reservations.DTOs.ReservationDto>.Create([], 0, page, pageSize)));
    }

    [HttpPut("restaurants/{restaurantId:guid}/opening-hours")]
    public async Task<IActionResult> UpsertOpeningHours(
        Guid restaurantId, [FromBody] List<OpeningHoursInputDto> hours, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        await mediator.Send(new UpsertOpeningHoursCommand(restaurantId, userId, role, hours), ct);
        return Ok(ApiResponse.Ok("Opening hours updated."));
    }
}
