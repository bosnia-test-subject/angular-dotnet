using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
public class AccountController : BaseApiController
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountController> _logger;
    public AccountController(IAccountService accountService, ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        _logger.LogDebug("Register endpoint called for username: {Username}", registerDto.Username);

        try
        {
            var user = await _accountService.RegisterAsync(registerDto);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while registering user: {Username}", registerDto.Username);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while registering user: {Username}", registerDto.Username);
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        _logger.LogDebug("Login endpoint called for username: {Username}", loginDto.Username);

        try
        {
            var user = await _accountService.LoginAsync(loginDto);
            return Ok(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access while logging in user: {Username}", loginDto.Username);
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while logging in user: {Username}", loginDto.Username);
            return StatusCode(500, "Internal server error");
        }
    }
}