using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers;

[Authorize(Policy = "RequireAdminRole")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }
    [HttpPost("create-tag")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto dto)
    {
        var tagName = dto?.Name?.Trim();
        if (string.IsNullOrWhiteSpace(tagName))
        {
            _logger.LogWarning("Tag name is null or empty.");
            return BadRequest(new { message = "Tag name cannot be null or empty." });
        }
        try
        {
            _logger.LogDebug("Admin Controller has been initiated - Method CreateTag()");
            if (dto == null || string.IsNullOrWhiteSpace(tagName))
            {
                _logger.LogWarning("Invalid tag data provided.");
                return BadRequest(new { message = "Invalid tag data." });
            }

            var tag = await _adminService.CreateTagAsync(tagName);
            return Ok(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating tag with name: {TagName}", tagName);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    [HttpGet("get-tags")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<object>>> GetTags()
    {
        try
        {
            _logger.LogDebug("Admin Controller has been initiated - Method GetTags()");
            var tags = await _adminService.GetTagsAsync();
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tags.");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("unapproved-photos")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetPhotosForApproval()
    {
        try
        {
            _logger.LogDebug("Admin Controller has been initiated - Method GetPhotos()");
            var photos = await _adminService.GetPhotosForApprovalAsync();
            return Ok(photos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching photos for approval.");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    [HttpPost("approve-photo/{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ApprovePhoto(int id)
    {
        try
        {
            _logger.LogDebug("ApprovePhoto called for PhotoId: {PhotoId}", id);
            await _adminService.ApprovePhotoAsync(id);
            return Ok(new { message = "Photo successfully approved." });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Photo not found with ID: {PhotoId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while approving photo with ID: {PhotoId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    [HttpDelete("reject-photo/{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RejectPhoto(int id)
    {
        try
        {
            _logger.LogDebug("RejectPhoto called for PhotoId: {PhotoId}", id);
            await _adminService.RejectPhotoAsync(id);
            return Ok(new { message = "Photo successfully rejected." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while rejecting photo with ID: {PhotoId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rejecting photo with ID: {PhotoId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    [HttpGet("users-with-roles")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        try
        {
            var users = await _adminService.GetUsersWithRolesAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching users with roles.");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    [HttpPost("edit-roles/{username}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        try
        {
            await _adminService.EditRolesAsync(username, roles);
            return Ok(new { message = "Roles successfully updated." });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input while editing roles for user: {Username}", username);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found while editing roles for user: {Username}", username);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update roles for user: {Username}", username);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while editing roles for user: {Username}", username);
            return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
        }
    }
    [HttpGet("photo-stats")]
    [ProducesResponseType(typeof(IEnumerable<PhotoStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PhotoStatsDto>>> GetPhotoApprovalStats()
    {
        try
        {
            _logger.LogDebug("Admin Controller has been initiated - Method GetPhotoApprovalStats()");
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                _logger.LogWarning("Current user ID is null.");
                return BadRequest(new { message = "Current user ID is required." });
            }

            var stats = await _adminService.GetPhotoApprovalStatsAsync(int.Parse(currentUserId));
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching photo approval stats.");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    [HttpGet("users-without-main-photo")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetUsersWithoutMainPhoto()
    {
        try
        {
            _logger.LogDebug("Admin Controller has been initiated - Method GetUsersWithoutMainPhoto()");
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                _logger.LogWarning("Current user ID is null.");
                return BadRequest(new { message = "Current user ID is required." });
            }

            var users = await _adminService.GetUsersWithoutMainPhotoAsync(int.Parse(currentUserId));
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching users without main photo.");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}