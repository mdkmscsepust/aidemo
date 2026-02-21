using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationPlatform.Application.Common.Models;
using ReservationPlatform.Application.Tables.Commands.CreateTable;

namespace ReservationPlatform.API.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId:guid}/tables")]
[Authorize(Policy = "OwnerOrAdmin")]
public class TablesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        Guid restaurantId, [FromBody] CreateTableRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var command = new CreateTableCommand(
            restaurantId, userId, role, request.TableNumber,
            request.Capacity, request.MinCapacity, request.Notes);
        var result = await mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<Application.Restaurants.DTOs.TableDto>.Ok(result));
    }
}

public record CreateTableRequest(string TableNumber, int Capacity, int MinCapacity = 1, string? Notes = null);
