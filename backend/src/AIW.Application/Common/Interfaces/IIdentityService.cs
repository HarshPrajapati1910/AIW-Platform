using AIW.Application.Auth.Models;

namespace AIW.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Succeeded, string? Error, AuthResponse? Response)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error, AuthResponse? Response)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}