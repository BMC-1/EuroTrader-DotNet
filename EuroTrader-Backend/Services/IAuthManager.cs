using EuroTrader_Backend.Models.Account;
using Microsoft.AspNetCore.Identity;

namespace EuroTrader_Backend.Services;

public interface IAuthManager
{
        Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto);
        Task<AuthResponseDto> Login(LoginDto userDto);
        Task<string> CreateRefreshToken();
        Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request);
}