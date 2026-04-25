using LMS.API.DTOs;
using LMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid input."));

        var (success, message, data) = await _authService.RegisterAsync(dto);
        if (!success)
            return BadRequest(ApiResponse<string>.Fail(message));

        return Ok(ApiResponse<LoginResponseDto>.Ok(data!, message));
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (success, message, data) = await _authService.LoginAsync(dto);
        if (!success)
            return Unauthorized(ApiResponse<string>.Fail(message));

        return Ok(ApiResponse<LoginResponseDto>.Ok(data!, message));
    }

    // GET /api/auth/me  (requires valid JWT)
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")!);
        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
            return Unauthorized(ApiResponse<string>.Fail("User not found."));

        return Ok(ApiResponse<UserDto>.Ok(user));
    }
}
