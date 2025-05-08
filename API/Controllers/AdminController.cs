using System;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService) : BaseApiController
{

    // PHOTO MANAGEMENT TASK NUM. 10.
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("unapproved-photos/{username}")]
    public async Task<ActionResult> GetPhotosForApproval(string username) 
    {
        var user = await userManager.FindByNameAsync(username);

        if(user == null) return BadRequest("User not found");

        var photos = await unitOfWork.PhotosRepository.GetUnapprovedPhotos(user.UserName!);
        return Ok(photos);
    }
    // PHOTO MANAGEMENT TASK NUM. 12.
    [Authorize(Policy = "RequireAdminRole")]
    [HttpDelete("reject-photo/{id}")]
    public async Task<ActionResult> RejectPhoto(int id) 
    {
        var photo = await unitOfWork.PhotosRepository.GetPhotoById(id);

        if(photo == null || photo.IsMain) return BadRequest("This photo cannot be rejected!");

        if(photo.PublicId != null) 
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null) return BadRequest(result.Error.Message);
        }

        unitOfWork.PhotosRepository.RemovePhoto(photo);

        if (await unitOfWork.Complete()) return Ok();
        return BadRequest("Problem rejecting photo!");
    }

    // PHOTO MANAGEMENT TASK NUM. 11.
    [Authorize(Policy ="RequireAdminRole")]
    [HttpPost("approve-photo/{id}")]
    public async Task<ActionResult> ApprovePhoto(int id) 
    {
        var photo = await unitOfWork.PhotosRepository.GetPhotoById(id);

        if(photo == null) return BadRequest("Photo not found!");

        photo.isApproved = true;

        var user = await unitOfWork.UserRepository.GetUserByIdAsync(photo.AppUserId);

        if (user == null) return BadRequest("User not found!");

        // Task num. 14.
        if (!user.Photos.Any(p => p.IsMain)) 
        {
            photo.IsMain = true;
        }

        if(await unitOfWork.Complete()) return Ok("Photo succesfully approved");

        return BadRequest("Problem approving photo!");
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles() 
    {
        var users = await userManager.Users
        .OrderBy(x => x.UserName)
        .Select(x => new 
        {
            x.Id,
            Username = x.UserName,
            Roles = x.UserRoles.Select(r => r.Role.Name).ToList(),
        }).ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, string roles) 
    {
        if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role!");

        var selectedRoles = roles.Split(",").ToArray();

        var user = await userManager.FindByNameAsync(username);

        if(user == null) return BadRequest("User not found");

        var userRoles = await userManager.GetRolesAsync(user);

        var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if(!result.Succeeded) return BadRequest("Failed to add to roles");

        result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if(!result.Succeeded) return BadRequest("Failed to remove from roles");

        return Ok(await userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotosForModeration() 
    {
        return Ok("Only admins and moderators can see this");
    }
}
