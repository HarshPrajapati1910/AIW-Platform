namespace AIW.Application.Auth.Models;

public record RegisterRequest(string Email, string Password, string Role = "Member");
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Email, string Role, Guid UserId);