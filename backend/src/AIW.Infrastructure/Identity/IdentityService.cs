using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AIW.Application.Auth.Models;
using AIW.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AIW.Infrastructure.Identity;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IConfiguration config) : IIdentityService
{
    public async Task<(bool Succeeded, string? Error, AuthResponse? Response)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return (false, "Email is already registered.", null);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)), null);

        var targetRole = string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Member";
        if (!await roleManager.RoleExistsAsync(targetRole))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(targetRole));
        }

        await userManager.AddToRoleAsync(user, targetRole);
        var token = GenerateJwtToken(user, targetRole);

        return (true, null, new AuthResponse(token, user.Email!, targetRole, user.Id));
    }

    public async Task<(bool Succeeded, string? Error, AuthResponse? Response)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
            return (false, "Invalid credentials.", null);

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Member";
        var token = GenerateJwtToken(user, role);

        return (true, null, new AuthResponse(token, user.Email!, role, user.Id));
    }

    private string GenerateJwtToken(ApplicationUser user, string role)
    {
        var secret = config["Jwt:SecretKey"] ?? "AIW_Platform_Super_Secret_Key_32_Bytes_Long_Minimum!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"] ?? "AIW-Platform",
            audience: config["Jwt:Audience"] ?? "AIW-Platform-Client",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}