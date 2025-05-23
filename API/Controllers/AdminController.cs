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

    /// <summary>
    /// Gets all photos that are pending approval.
    /// </summary>
    /// <returns>List of unapproved photos.</returns>
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

    /// <summary>
    /// Approves a photo by its ID.
    /// </summary>
    /// <param name="id">The ID of the photo to approve.</param>
    /// <returns>Status message.</returns>
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

    /// <summary>
    /// Rejects a photo by its ID.
    /// </summary>
    /// <param name="id">The ID of the photo to reject.</param>
    /// <returns>Status message.</returns>
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

    /// <summary>
    /// Gets all users with their roles.
    /// </summary>
    /// <returns>List of users and their roles.</returns>
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

    /// <summary>
    /// Edits the roles for a specific user.
    /// </summary>
    /// <param name="username">The username of the user to edit roles for.</param>
    /// <param name="roles">Comma-separated list of roles to assign.</param>
    /// <returns>Status message.</returns>
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
            return Ok (new { message = "Roles successfully updated." });
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
}