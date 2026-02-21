using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationPlatform.Application.Common.Models;
using ReservationPlatform.Application.Reservations.Queries.GetAvailability;
using ReservationPlatform.Application.Restaurants.Commands.CreateRestaurant;
using ReservationPlatform.Application.Restaurants.Commands.DeleteRestaurant;
using ReservationPlatform.Application.Restaurants.Commands.UpdateRestaurant;
using ReservationPlatform.Application.Restaurants.Queries.GetRestaurantById;
using ReservationPlatform.Application.Restaurants.Queries.GetRestaurants;
using ReservationPlatform.Application.Reviews.Commands.CreateReview;
using ReservationPlatform.Application.Reviews.Queries.GetRestaurantReviews;
using ReservationPlatform.Domain.Enums;

namespace ReservationPlatform.API.Controllers;

[ApiController]
[Route("api/restaurants")]
public class RestaurantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? search, [FromQuery] string? city, [FromQuery] string? cuisine,
        [FromQuery] int? priceMin, [FromQuery] int? priceMax,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string sortBy = "rating",
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetRestaurantsQuery(
            search, city, cuisine, priceMin, priceMax, page, pageSize, sortBy), ct);
        return Ok(ApiResponse<PaginatedResult<Application.Restaurants.DTOs.RestaurantListDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetRestaurantByIdQuery(id), ct);
        return Ok(ApiResponse<Application.Restaurants.DTOs.RestaurantDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "OwnerOrAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateRestaurantRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateRestaurantCommand(
            userId, request.Name, request.Description, request.CuisineType,
            request.AddressLine1, request.AddressLine2, request.City, request.State,
            request.PostalCode, request.Country ?? "US", request.Phone, request.Email,
            request.PriceTier, request.DefaultDurationMinutes);
        var result = await mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<Application.Restaurants.DTOs.RestaurantListDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "OwnerOrAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRestaurantRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var command = new UpdateRestaurantCommand(
            id, userId, role, request.Name, request.Description, request.CuisineType,
            request.AddressLine1, request.AddressLine2, request.City, request.State,
            request.PostalCode, request.Phone, request.Email, request.Website,
            request.PriceTier, request.DefaultDurationMinutes, request.ImageUrl);
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<Application.Restaurants.DTOs.RestaurantListDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "OwnerOrAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        await mediator.Send(new DeleteRestaurantCommand(id, userId, role), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/availability")]
    public async Task<IActionResult> GetAvailability(
        Guid id, [FromQuery] string date, [FromQuery] int partySize, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAvailabilityQuery(id, date, partySize), ct);
        return Ok(ApiResponse<List<Application.Reservations.DTOs.AvailableSlotDto>>.Ok(result));
    }

    [HttpGet("{id:guid}/reviews")]
    public async Task<IActionResult> GetReviews(
        Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetRestaurantReviewsQuery(id, page, pageSize), ct);
        return Ok(ApiResponse<PaginatedResult<Application.Reviews.DTOs.ReviewDto>>.Ok(result));
    }

    [HttpPost("{id:guid}/reviews")]
    [Authorize(Policy = "CustomerOnly")]
    public async Task<IActionResult> CreateReview(
        Guid id, [FromBody] CreateReviewRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateReviewCommand(id, userId, request.ReservationId, request.Rating, request.Comment);
        var result = await mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<Application.Reviews.DTOs.ReviewDto>.Ok(result));
    }
}

public record CreateRestaurantRequest(
    string Name, string? Description, string? CuisineType,
    string AddressLine1, string? AddressLine2, string City, string? State,
    string PostalCode, string? Country, string? Phone, string? Email,
    PriceTier PriceTier, int DefaultDurationMinutes = 90);

public record UpdateRestaurantRequest(
    string Name, string? Description, string? CuisineType,
    string AddressLine1, string? AddressLine2, string City, string? State,
    string PostalCode, string? Phone, string? Email, string? Website,
    PriceTier PriceTier, int DefaultDurationMinutes, string? ImageUrl);

public record CreateReviewRequest(Guid ReservationId, int Rating, string? Comment);
