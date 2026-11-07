using AIW.Application.Auth.Models;
using AIW.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIW.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IIdentityService identityService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var (succeeded, error, response) = await identityService.RegisterAsync(request, ct);
        if (!succeeded) return BadRequest(new { message = error });
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var (succeeded, error, response) = await identityService.LoginAsync(request, ct);
        if (!succeeded) return Unauthorized(new { message = error });
        return Ok(response);
    }
}