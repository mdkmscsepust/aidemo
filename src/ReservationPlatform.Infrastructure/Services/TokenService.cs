using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ReservationPlatform.Application.Auth.Commands.Register;
using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Infrastructure.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    private readonly string _secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
    private readonly string _issuer = configuration["Jwt:Issuer"] ?? "ReservationPlatform";
    private readonly string _audience = configuration["Jwt:Audience"] ?? "ReservationPlatformClient";
    private readonly int _accessTokenExpiryMinutes = int.Parse(configuration["Jwt:AccessTokenExpiryMinutes"] ?? "15");
    private readonly int _refreshTokenExpiryDays = int.Parse(configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string Token, DateTime ExpiresAt) GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        var token = Convert.ToBase64String(bytes);
        var expiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);
        return (token, expiresAt);
    }

    public DateTime GetAccessTokenExpiry() =>
        DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes);
}
