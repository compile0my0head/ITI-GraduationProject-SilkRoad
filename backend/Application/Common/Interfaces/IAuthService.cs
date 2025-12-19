using Application.DTOs.Auth;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<UserResponseDto> LoginAsync(LoginRequestDto loginDto);

    Task<UserResponseDto> RegisterAsync(RegisterRequestDto registerDto);   
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task LogoutAsync(int userId, CancellationToken cancellationToken = default);
}
