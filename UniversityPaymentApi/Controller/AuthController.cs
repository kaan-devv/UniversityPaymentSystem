using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UniversityPaymentApi.Dtos;

namespace UniversityPaymentApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    private readonly Dictionary<string, (string Password, string Role)> _users =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "admin", ("Admin123!", "Admin") },
            { "bankuser", ("Bank123!", "Bank") }
        };

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("token")]
    [AllowAnonymous]
    public ActionResult<TokenResponseDto> GetToken([FromBody] LoginRequestDto request)
    {
        if (!_users.TryGetValue(request.Username, out var userInfo) ||
            userInfo.Password != request.Password)
        {
            return Unauthorized(new TokenResponseDto
            {
                Success = false,
                Message = "Invalid username or password."
            });
        }

        var role = userInfo.Role;

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new TokenResponseDto
        {
            Success = true,
            Message = "Token generated.",
            Token = tokenString,
            ExpiresAt = token.ValidTo,
            Role = role
        });
    }
}
