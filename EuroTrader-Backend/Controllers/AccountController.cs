using EuroTrader_Backend.Models.Account;
using EuroTrader_Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace EuroTrader_Backend.Controllers;

/// <summary>
/// Account Controller
/// 1.Login Method
/// 2.Registration Method
/// 3.Refresh Token Method
/// </summary>
[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAuthManager _authManager;

    public AccountController(ILogger<AccountController> logger,IAuthManager authManager)
    {
        _logger = logger;
        _authManager = authManager;
    }

    // POST: api/Account/Login
    [HttpPost]
    [Route("Login")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Login([FromBody] LoginDto userDto)
    {
        _logger.LogInformation("Login Attempt for {UserDtoUsername}", userDto.Username);
        try
        {
            var authResponse = await _authManager.Login(userDto);

            if (authResponse == null)
            {
                //401 - No access ( check username / password )
                return Unauthorized();

                //403 - No authorize to do this ( may a role or permission )
                //return Forbid();
            }

            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            //Logger showing this on the running app
            _logger.LogError(ex, "Something went wrong in the {RegisterUserName} - " +
                                 "User Login attempt for {UserDtoUsername}", nameof(Login), userDto.Username);
            //Return problem from API side...
            return Problem(
                $"Something went wrong in the {nameof(Login)}.{Environment.NewLine} Please contact support",
                statusCode: 500);
        }
    }

    // // POST: api/Account/Register
    [HttpPost]
    [Route("Register")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RegisterUser([FromBody] ApiUserDto userDto)
    {
        _logger.LogInformation("Registration Attempt for {UserDtoUsername}", userDto.Username);
    
        try
        {
            var errors = await _authManager.Register(userDto);
            var identityErrors = errors.ToList();
            if (!identityErrors.Any()) return Ok();
            foreach (var error in identityErrors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
    
            return BadRequest(ModelState);
    
        }
        catch (Exception ex)
        {
            //Logger showing this on the running app
            _logger.LogError(ex, "Something went wrong in the {RegisterUserName} - " +
                                 "User Registration attempt for {UserDtoUsername}", nameof(RegisterUser),
                userDto.Username);
            //Return problem from API side...
            return Problem(
                $"Something went wrong in the {nameof(RegisterUser)}.{Environment.NewLine} Please contact support",
                statusCode: 500);
        }
    }
}