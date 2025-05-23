using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        var username = User.GetUsername();
        if (string.IsNullOrWhiteSpace(username) || username == null)
        {
            _logger.LogWarning("Authenticated username is missing or invalid.");
            return NotFound("Authenticated user not found.");
        }

        try
        {
            userParams.CurrentUsername = username;
            var users = await _userService.GetUsersAsync(userParams);
            Response.AddPaginationHeader(users);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching users with user parameters: {@UserParams}", userParams);
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet("{username}")]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        if (string.IsNullOrWhiteSpace(username) || username == null)
        {
            _logger.LogWarning("Requested username is missing or invalid.");
            return NotFound("User not found.");
        }

        try
        {
            var user = await _userService.GetUserAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User with username {Username} not found.", username);
                return NotFound($"User with username {username} not found.");
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user with username: {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var username = User.GetUsername();
        if (string.IsNullOrWhiteSpace(username) || username == null)
        {
            _logger.LogWarning("Authenticated username is missing or invalid.");
            return NotFound("Authenticated user not found.");
        }

        try
        {
            await _userService.UpdateUserAsync(username, memberUpdateDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User with username {Username} not found.", username);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user with username: {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpPost("add-photo")]
    [ProducesResponseType(typeof(PhotoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var username = User.GetUsername();
        if (string.IsNullOrWhiteSpace(username) || username == null)
        {
            _logger.LogWarning("Authenticated username is missing or invalid.");
            return NotFound("Authenticated user not found.");
        }

        try
        {
            var photo = await _userService.AddPhotoAsync(username, file);
            return CreatedAtAction(nameof(GetUser), new { username }, photo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding photo for user: {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpPut("set-main-photo/{photoId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var username = User.GetUsername();
        if (string.IsNullOrWhiteSpace(username) || username == null)
        {
            _logger.LogWarning("Authenticated username is missing or invalid.");
            return NotFound("Authenticated user not found.");
        }

        try
        {
            await _userService.SetMainPhotoAsync(username, photoId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Photo with ID {PhotoId} not found for user: {Username}", photoId, username);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting main photo for user: {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpDelete("delete-photo/{photoId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var username = User.GetUsername();
        if (string.IsNullOrWhiteSpace(username) || username == null)
        {
            _logger.LogWarning("Authenticated username is missing or invalid.");
            return NotFound("Authenticated user not found.");
        }

        try
        {
            await _userService.DeletePhotoAsync(username, photoId);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Photo with ID {PhotoId} not found for user: {Username}", photoId, username);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting photo for user: {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }
}