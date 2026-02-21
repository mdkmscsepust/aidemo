using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReservationPlatform.Application.Auth.Commands.Login;
using ReservationPlatform.Application.Auth.Commands.Logout;
using ReservationPlatform.Application.Auth.Commands.RefreshToken;
using ReservationPlatform.Application.Auth.Commands.Register;
using ReservationPlatform.Application.Common.Models;
using ReservationPlatform.Domain.Enums;

namespace ReservationPlatform.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<Application.Auth.DTOs.AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterCommand(
            request.Email, request.Password, request.FirstName, request.LastName,
            request.Phone, request.Role ?? UserRole.Customer);
        var result = await mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<Application.Auth.DTOs.AuthResponseDto>.Ok(result, "Registration successful."));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<Application.Auth.DTOs.AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password), ct);
        return Ok(ApiResponse<Application.Auth.DTOs.AuthResponseDto>.Ok(result, "Login successful."));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<Application.Auth.DTOs.AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RefreshTokenCommand(request.RefreshToken), ct);
        return Ok(ApiResponse<Application.Auth.DTOs.AuthResponseDto>.Ok(result));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken ct)
    {
        await mediator.Send(new LogoutCommand(request.RefreshToken), ct);
        return NoContent();
    }
}

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone,
    UserRole? Role);

public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
