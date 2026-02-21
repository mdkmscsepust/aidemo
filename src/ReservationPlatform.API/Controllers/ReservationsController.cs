using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationPlatform.Application.Common.Models;
using ReservationPlatform.Application.Reservations.Commands.CancelReservation;
using ReservationPlatform.Application.Reservations.Commands.CreateReservation;
using ReservationPlatform.Application.Reservations.Queries.GetMyReservations;
using ReservationPlatform.Application.Reservations.Queries.GetReservationById;
using ReservationPlatform.Domain.Enums;

namespace ReservationPlatform.API.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "CustomerOnly")]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateReservationCommand(
            request.RestaurantId, request.TableId, userId,
            request.ReservationDate, request.SlotTime, request.PartySize, request.SpecialRequests);
        var result = await mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<Application.Reservations.DTOs.ReservationDto>.Ok(result, "Reservation confirmed."));
    }

    [HttpGet]
    public async Task<IActionResult> GetMine(
        [FromQuery] ReservationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new GetMyReservationsQuery(userId, status, page, pageSize), ct);
        return Ok(ApiResponse<PaginatedResult<Application.Reservations.DTOs.ReservationDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var result = await mediator.Send(new GetReservationByIdQuery(id, userId, role), ct);
        return Ok(ApiResponse<Application.Reservations.DTOs.ReservationDto>.Ok(result));
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var result = await mediator.Send(new CancelReservationCommand(id, userId, role, request.Reason), ct);
        return Ok(ApiResponse<Application.Reservations.DTOs.ReservationDto>.Ok(result, "Reservation cancelled."));
    }
}

public record CreateReservationRequest(
    Guid RestaurantId, Guid TableId,
    string ReservationDate, string SlotTime,
    int PartySize, string? SpecialRequests);

public record CancelRequest(string? Reason);
