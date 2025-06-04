using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AdminService> _logger;

        public AdminService(IUnitOfWork unitOfWork, IPhotoService photoService, UserManager<AppUser> userManager, ILogger<AdminService> logger)
        {
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<IEnumerable<object>> GetTagsAsync()
        {
            try
            {
                return await _unitOfWork.PhotosRepository.GetTags();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching tags.");
                throw;
            }
        }
        public async Task<object> CreateTagAsync(string tagName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tagName))
                {
                    throw new ArgumentException("Tag name cannot be null or empty.");
                }

                var tag = new Tag { Name = tagName };

                var existingTags = await _unitOfWork.TagsRepository.GetAllTagsAsync();
                if (existingTags.Any(t => t.Name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return "duplicate";
                }

                _unitOfWork.PhotosRepository.AddTag(tag);

                if (!await _unitOfWork.Complete())
                {
                    throw new Exception("Problem creating tag.");
                }

                return tag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating tag: {tagName}", tagName);
                throw;
            }
        }
        public async Task<IEnumerable<object>> GetPhotosForApprovalAsync()
        {
            try
            {
                return await _unitOfWork.PhotosRepository.GetUnapprovedPhotos();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching photos for approval.");
                throw;
            }
        }

        public async Task ApprovePhotoAsync(int id)
        {
            try
            {
                var photo = await _unitOfWork.PhotosRepository.GetPhotoById(id);
                if (photo == null) throw new KeyNotFoundException("Photo not found.");

                photo.isApproved = true;

                var user = await _unitOfWork.UserRepository.GetUserByIdAsync(photo.AppUserId);
                if (user == null) throw new KeyNotFoundException("User not found.");

                if (!user.Photos.Any(p => p.IsMain))
                {
                    photo.IsMain = true;
                }

                if (!await _unitOfWork.Complete())
                {
                    throw new Exception("Problem approving photo.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while approving photo with ID: {id}", id);
                throw;
            }
        }

        public async Task RejectPhotoAsync(int id)
        {
            try
            {
                var photo = await _unitOfWork.PhotosRepository.GetPhotoById(id);
                if (photo == null || photo.IsMain) throw new InvalidOperationException("This photo cannot be rejected.");

                if (photo.PublicId != null)
                {
                    var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                    if (result.Error != null) throw new Exception(result.Error.Message);
                }

                _unitOfWork.PhotosRepository.RemovePhoto(photo);

                if (!await _unitOfWork.Complete())
                {
                    throw new Exception("Problem rejecting photo.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while rejecting photo with ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<object>> GetUsersWithRolesAsync()
        {
            try
            {
                return await _userManager.Users
                    .OrderBy(x => x.UserName)
                    .Select(x => new
                    {
                        x.Id,
                        Username = x.UserName,
                        Roles = x.UserRoles.Select(r => r.Role.Name).ToList(),
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users with roles.");
                throw;
            }
        }

        public async Task EditRolesAsync(string username, string roles)
        {
            try
            {
                if (string.IsNullOrEmpty(roles))
                    throw new ArgumentException("You must select at least one role.");

                var selectedRoles = roles.Split(",").ToArray();

                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                    throw new KeyNotFoundException("User not found.");

                var userRoles = await _userManager.GetRolesAsync(user);

                var addToRolesResult = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
                if (!addToRolesResult.Succeeded)
                {
                    var errors = string.Join(", ", addToRolesResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to add roles: {errors}");
                }

                var removeFromRolesResult = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
                if (!removeFromRolesResult.Succeeded)
                {
                    var errors = string.Join(", ", removeFromRolesResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to remove roles: {errors}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while editing roles for user: {username}", username);
                throw;
            }
        }
        public async Task<List<PhotoStatsDto>> GetPhotoApprovalStatsAsync(int currentUserId)
        {
            try
            {
                var stats = await _unitOfWork.PhotosRepository.GetPhotoApprovalStatsAsync(currentUserId);
                if (stats == null || !stats.Any())
                {
                    throw new KeyNotFoundException("No photo approval stats found for the current user.");
                }
                return stats.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching photo approval stats for user ID: {currentUserId}", currentUserId);
                throw;
            }
        }
        public async Task<List<string>> GetUsersWithoutMainPhotoAsync(int currentUserId)
        {
            try
            {
                var usersWithoutMainPhoto = await _unitOfWork.PhotosRepository.GetUsersWithoutMainPhotoAsync(currentUserId);
                if (usersWithoutMainPhoto == null || !usersWithoutMainPhoto.Any())
                {
                    throw new KeyNotFoundException("No users without main photo found.");
                }
                return usersWithoutMainPhoto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users without main photo for user ID: {currentUserId}", currentUserId);
                throw;
            }
        }
        public async Task RemoveTagByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tag name is required.");
            try
            {
                await _unitOfWork.TagsRepository.RemoveTagByName(name);
                if (!await _unitOfWork.Complete())
                {
                    throw new Exception("Failed to remove tag.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing tag: {name}", name);
                throw;
            }
        }
    }
}