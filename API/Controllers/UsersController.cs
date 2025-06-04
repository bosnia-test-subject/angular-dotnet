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
    [HttpPost("assign-tags/{photoId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AssignTags(int photoId, [FromBody] List<string> tags)
    {
        var username = User.GetUsername();
        if (string.IsNullOrWhiteSpace(username) || username == null)
        {
            _logger.LogWarning("Authenticated username is missing or invalid.");
            return NotFound("Authenticated user not found.");
        }
        try
        {
            await _userService.AssignTagsByNameAsync(username, photoId, tags);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while assigning tags to photo with ID {PhotoId} for user: {Username}", photoId, username);
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Photo with ID {PhotoId} not found for user: {Username}", photoId, username);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning tags to photo for user: {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet("photos-tags")]
    [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<PhotoDto>>> GetPhotoTagsByUsername()
    {
        var username = User.GetUsername();
        if (string.IsNullOrWhiteSpace(username) || username == null)
        {
            _logger.LogWarning("Authenticated username is missing or invalid.");
            return NotFound("Authenticated user not found.");
        }
        try
        {
            var photos = await _userService.GetPhotoWithTagsByUsernameAsync(username);
            if (photos == null)
            {
                _logger.LogWarning("No tags found for user with username {username}.", username);
                return NotFound($"No tags found for username {username}.");
            }
            return Ok(photos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tags for username {username}.", username);
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet("tags")]
    [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetTags()
    {
        try
        {
            _logger.LogDebug("Admin Controller has been initiated - Method GetTags()");
            var tags = await _userService.GetTagsAsync();
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tags.");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    [HttpDelete("remove-tag/{photoId:int}/{tagName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemoveTagFromPhoto(int photoId, string tagName)
    {
        var username = User.GetUsername();
        if (string.IsNullOrWhiteSpace(username) || username == null)
        {
            _logger.LogWarning("Authenticated username is missing or invalid.");
            return NotFound("Authenticated user not found.");
        }
        try
        {
            await _userService.RemoveTagFromPhotoAsync(username, photoId, tagName);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Photo with ID {PhotoId} or tag {TagName} not found for user: {Username}", photoId, tagName, username);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing tag from photo for user: {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }
}