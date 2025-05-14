using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Policy = "RequireAdminRole")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet("unapproved-photos")]
    public async Task<ActionResult> GetPhotosForApproval()
    {
        try
        {
            var photos = await _adminService.GetPhotosForApprovalAsync();
            return Ok(photos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching photos for approval.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("approve-photo/{id}")]
    public async Task<ActionResult> ApprovePhoto(int id)
    {
        try
        {
            await _adminService.ApprovePhotoAsync(id);
            return Ok("Photo successfully approved.");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Photo not found with ID: {id}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while approving photo with ID: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("reject-photo/{id}")]
    public async Task<ActionResult> RejectPhoto(int id)
    {
        try
        {
            await _adminService.RejectPhotoAsync(id);
            return Ok("Photo successfully rejected.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while rejecting photo with ID: {id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rejecting photo with ID: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("users-with-roles")]
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
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        try
        {
            await _adminService.EditRolesAsync(username, roles);
            return Ok("Roles successfully updated.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input while editing roles for user: {username}", username);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found while editing roles for user: {username}", username);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update roles for user: {username}", username);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while editing roles for user: {username}", username);
            return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
        }
    }
}