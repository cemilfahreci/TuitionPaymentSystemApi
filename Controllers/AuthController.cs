using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TuitionApi.Data;
using TuitionApi.Models;

namespace TuitionApi.Controllers;

[Route("api/v1/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly TuitionDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(TuitionDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Demo Auth Logic
        if (request.Username == "admin" && request.Password == "admin")
        {
            var token = GenerateJwtToken(request.Username);
            return Ok(new { access_token = token });
        }

        var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
        // In real app, verify hash. Here assuming plain for simplicity or pre-hashed check
        // But let's stick to the demo "admin/admin" mostly or simple check
        if (user != null && user.PasswordHash == request.Password) 
        {
            var token = GenerateJwtToken(user.Username);
            return Ok(new { access_token = token });
        }

        return Unauthorized(new { msg = "Bad username or password" });
    }

    private string GenerateJwtToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super-secret-key-change-in-production-must-be-long-enough"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(ClaimTypes.Name, username) };

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
