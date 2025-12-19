using Application.Common.Configuration;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services;

public class AuthService(UserManager<User> _userManager , IJwtService jwtService , IUnitOfWork unitOfWork, IOptions<JwtOptions> options) :IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;


    private async Task<string> CreateTokenAsync(User user)
    {
        var jwtOptions = options.Value;
        // Claims
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier , user.Id.ToString())

            };
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        // Key 
        var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

        // Credentials
        var credentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken
        (
            claims: claims,
            expires: DateTime.Now.AddDays(jwtOptions.DurationInDays),
            signingCredentials: credentials,
            audience: jwtOptions.Audience,
            issuer: jwtOptions.Issuer

        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto registerDto)
    {
        var user = new User
        {
            UserName = registerDto.Email, // Add this line!
            Email = registerDto.Email,
            FullName = registerDto.FullName

        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();

            throw new ValidationException(errors);
        }
        return new UserResponseDto
       (
           user.FullName,
           user.Email!,
           await CreateTokenAsync(user)
       );

    }

    public async Task<UserResponseDto> LoginAsync(LoginRequestDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user is null)
            throw new UnAuthorizedException($"Email {loginDto.Email} Is Not Exsist!");

        var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result)
            throw new UnAuthorizedException("Password Is Incorrect!");

        return new UserResponseDto
        (
            user.FullName,
            user.Email!,
            await CreateTokenAsync(user)
        );
    }
    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task LogoutAsync(int userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
