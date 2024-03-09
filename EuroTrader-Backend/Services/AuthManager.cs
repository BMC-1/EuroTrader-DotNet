using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Data;
using Domain;
using EuroTrader_Backend.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace EuroTrader_Backend.Services;

/// <summary>
/// 1 -> Registration : Sets up the username , Hashes the password field
/// 2 -> Login : Finds the email || username { is the same at this app },de-hashes the password,generates the token and refresh token
/// 3 -> Region Token : Creates , Verify , Generates Tokens { token , refresh token }
/// </summary>
public class AuthManager : IAuthManager
{
    private readonly IMapper _mapper;
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _appDbContext;
    private AppUser _user;

    public const string LoginProvider = "eurotraderapi";
    private const string RefreshToken = "RefreshToken";
    
    public AuthManager(IMapper mapper,UserManager<AppUser> userManager,IConfiguration configuration,AppDbContext _appDbContext)
    {
        _mapper = mapper;
        _userManager = userManager;
        _configuration = configuration;
        this._appDbContext = _appDbContext;
    }
    
    
    public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
    {
        //transaction for ( both tables / user - roles )
        var transaction = await _appDbContext.Database.BeginTransactionAsync();
        
        _user = _mapper.Map<AppUser>(userDto);
        
        //Username field 
        _user.UserName = userDto.Username;

        //Username field is the the email of this account field
        _user.Email = userDto.Email;

        //encrypts and store the password on this method by microsoft
        // hashes the password
        var result = await _userManager.CreateAsync(_user,userDto.Password);

        //Roles
         if (result.Succeeded)
         {
             var roleSucceeded =await _userManager.AddToRoleAsync(_user, userDto.Role);
             
             if (roleSucceeded.Succeeded)
             {
                await transaction.CommitAsync();
             }
         }
         else
         {
             await transaction.RollbackAsync();
         }
         
        return result.Errors;
    }
    
    public async Task<AuthResponseDto> Login(LoginDto userDto)
    {
        //get {username}
        //find by email or username based on the username input
        _user = await _userManager.FindByNameAsync(userDto.Username) ?? await _userManager.FindByEmailAsync(userDto.Username);
        
        //if not null -> Check password
        var isValidUser = _user != null && await _userManager.CheckPasswordAsync(_user, userDto.Password);

        //if null -> return error
        if (isValidUser == false)
        {
            return null;
        }
        
        var token = await GenerateToken();
        
        var userRoles = await _userManager.GetRolesAsync(_user);
        return new AuthResponseDto()
        {
            Token = token,
            RefreshToken = await CreateRefreshToken(),
            Roles = userRoles,
        };
    }

    #region Token

    public async Task<string> CreateRefreshToken()
    {
        await _userManager.RemoveAuthenticationTokenAsync(_user,LoginProvider,RefreshToken);

        var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, LoginProvider, RefreshToken);

        var result =
            await _userManager.SetAuthenticationTokenAsync(_user, LoginProvider, RefreshToken, newRefreshToken);

        return newRefreshToken;
    }
    
    public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
    {
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
        var username = tokenContent.Claims.ToList().FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
        
        if (username != null)
                _user = await _userManager.FindByNameAsync(username);

        var isValidRefreshToken =
            await _userManager.VerifyUserTokenAsync(_user, LoginProvider, RefreshToken, request.Token);

        if (isValidRefreshToken)
        {
            var token = await GenerateToken();
            return new AuthResponseDto()
            {
                Token = token,
                //UserId = _user.Id,
                RefreshToken = await CreateRefreshToken()
            };
        }

        await _userManager.UpdateSecurityStampAsync(_user);

        return null;
    }
    
    private async Task<string> GenerateToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:SecretForKey"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        //Generate information about the user
        var roles = await _userManager.GetRolesAsync(_user);

        var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
        
        var userClaims = await _userManager.GetClaimsAsync(_user);
        
        var claims = new List<Claim>
        {
            // new Claim(JwtRegisteredClaimNames.Sub, _user.Email!),
            // new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // new Claim("Role", userRoles.FirstOrDefault() ?? string.Empty),
            // new Claim("uid", _user.Id),
            new Claim(JwtRegisteredClaimNames.Sub, _user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName,
                _user.UserName),
            new Claim(JwtRegisteredClaimNames.GivenName,
                _user.UserName),
            // new Claim(JwtRegisteredClaimNames.FamilyName,
            //     _user.LastName),
            new Claim(JwtRegisteredClaimNames.UniqueName,
                _user.UserName),
        }.Union(userClaims).Union(roleClaims);

        //Generates a token to give back to the client
        var token = new JwtSecurityToken(
            //Rules that we need to set - all can be found on the appsettings
            issuer:  _configuration["Authentication:Issuer"],
            audience: _configuration["Authentication:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Authentication:DurationInMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    #endregion
}