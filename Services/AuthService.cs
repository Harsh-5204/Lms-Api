using LMS.API.Data;
using LMS.API.DTOs;
using LMS.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LMS.API.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, LoginResponseDto? Data)> RegisterAsync(RegisterDto dto);
    Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginDto dto);
    Task<UserDto?> GetUserByIdAsync(int id);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> RegisterAsync(RegisterDto dto)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email.ToLower());
        if (exists)
            return (false, "Email is already registered.", null);

        var user = new User
        {
            FullName     = dto.FullName.Trim(),
            Email        = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role         = "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = GenerateToken(user);
        return (true, "Registration successful.", new LoginResponseDto { Token = token, User = MapToDto(user) });
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower() && u.IsActive);

        if (user == null)
            return (false, "Invalid email or password.", null);

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return (false, "Invalid email or password.", null);

        var token = GenerateToken(user);
        return (true, "Login successful.", new LoginResponseDto { Token = token, User = MapToDto(user) });
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        return user == null ? null : MapToDto(user);
    }

    private string GenerateToken(User user)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpiryHours"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name,               user.FullName),
            new Claim(ClaimTypes.Role,               user.Role),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id       = user.Id,
        FullName = user.FullName,
        Email    = user.Email,
        Role     = user.Role
    };
}
