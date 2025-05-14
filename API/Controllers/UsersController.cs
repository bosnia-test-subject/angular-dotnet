using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly ILogger<UserService> _logger;

    public UsersController(IUserService userService, ILogger<UserService> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        try
        {
            userParams.CurrentUsername = User.GetUsername();
            var users = await _userService.GetUsersAsync(userParams);
            Response.AddPaginationHeader(users);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching users with user parameters: {userParams}", userParams);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        try
        {
            var user = await _userService.GetUserAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User with username {username} not found.", username);
                return NotFound($"User with username {username} not found.");
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user with username: {username}", username);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        try
        {
            await _userService.UpdateUserAsync(User.GetUsername(), memberUpdateDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User with username {username} not found.", User.GetUsername());
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user with username: {username}", User.GetUsername());
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        try
        {
            var photo = await _userService.AddPhotoAsync(User.GetUsername(), file);
            return CreatedAtAction(nameof(GetUser), new { username = User.GetUsername() }, photo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding photo for user: {username}", User.GetUsername());
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        try
        {
            await _userService.SetMainPhotoAsync(User.GetUsername(), photoId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Photo with ID {photoId} not found for user: {username}", photoId, User.GetUsername());
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting main photo for user: {username}", User.GetUsername());
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        try
        {
            await _userService.DeletePhotoAsync(User.GetUsername(), photoId);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Photo with ID {photoId} not found for user: {username}", photoId, User.GetUsername());
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting photo for user: {username}", User.GetUsername());
            return StatusCode(500, "Internal server error");
        }
    }
}