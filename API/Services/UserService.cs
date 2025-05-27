using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.Identity.Client;

namespace API.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IPhotoService photoService, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _mapper = mapper;
        }

        public async Task<PagedList<MemberDto>> GetUsersAsync(UserParams userParams)
        {
            try
            {
                var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
                if (users == null || !users.Any())
                {
                    _logger.LogWarning("No users found with user parameters: {userParams}", userParams);
                    throw new KeyNotFoundException("No users found.");
                }
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users with user parameters: {userParams}", userParams);
                throw;
            }
        }

        public async Task<MemberDto> GetUserAsync(string username)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetMemberAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("User not found with username: {username}", username);
                    throw new KeyNotFoundException($"User with username {username} not found.");
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user with username: {username}", username);
                throw;
            }
        }

        public async Task UpdateUserAsync(string username, MemberUpdateDto memberUpdateDto)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("User not found with username: {username}", username);
                    throw new KeyNotFoundException("User not found.");
                }

                _mapper.Map(memberUpdateDto, user);

                if (!await _unitOfWork.Complete())
                {
                    throw new Exception("Failed to update user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with username: {username}", username);
                throw;
            }
        }

        public async Task<PhotoDto> AddPhotoAsync(string username, IFormFile file)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
                if (user == null) throw new KeyNotFoundException("User not found.");

                var result = await _photoService.AddPhotoAsync(file);
                if (result.Error != null) throw new Exception(result.Error.Message);

                var photo = new Photo
                {
                    Url = result.SecureUrl.AbsoluteUri,
                    PublicId = result.PublicId,
                    isApproved = false
                };

                user.Photos.Add(photo);

                if (!await _unitOfWork.Complete())
                {
                    throw new Exception("Problem adding photo.");
                }

                return _mapper.Map<PhotoDto>(photo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding photo for user: {username}", username);
                throw;
            }
        }

        public async Task SetMainPhotoAsync(string username, int photoId)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
                if (user == null) throw new KeyNotFoundException("User not found.");

                var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
                if (photo == null || photo.IsMain) throw new InvalidOperationException("Cannot set this photo as main.");

                var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
                if (currentMain != null) currentMain.IsMain = false;

                photo.IsMain = true;

                if (!await _unitOfWork.Complete())
                {
                    throw new Exception("Problem setting main photo.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while setting main photo for user: {username}", username);
                throw;
            }
        }

        public async Task DeletePhotoAsync(string username, int photoId)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
                if (user == null) throw new KeyNotFoundException("User not found.");

                var photo = await _unitOfWork.PhotosRepository.GetPhotoById(photoId);
                if (photo == null || photo.IsMain) throw new InvalidOperationException("Cannot delete this photo.");

                if (photo.IsMain)
                {
                    _logger.LogWarning("Cannot delete main photo for user: {username}", username);
                    throw new InvalidOperationException("Cannot delete main photo.");
                }
                if (username != user.UserName)
                {
                    _logger.LogWarning("User {username} attempted to delete photo of another user.", username);
                    throw new UnauthorizedAccessException("You cannot delete photos of other users.");
                }
                if (photo.AppUserId != user.Id)
                {
                    _logger.LogWarning("Photo with ID {photoId} does not belong to user: {username}", photoId, username);
                    throw new UnauthorizedAccessException("You cannot delete photos that do not belong to you.");
                }

                if (photo.PublicId != null)
                {
                    var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                    if (result.Error != null) throw new Exception(result.Error.Message);
                }

                user.Photos.Remove(photo);

                if (!await _unitOfWork.Complete())
                {
                    throw new Exception("Error deleting photo.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting photo for user: {username}", username);
                throw;
            }
        }
        public async Task AssignTagsByNameAsync(string username, int photoId, List<string> tagNames)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required.");

            if (tagNames == null || !tagNames.Any())
                throw new ArgumentException("At least one tag name is required.");

            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            if (user == null) throw new KeyNotFoundException("User not found.");

            var photo = await _unitOfWork.PhotosRepository.GetPhotoWithTagsByIdAsync(photoId);
            if (photo == null) throw new KeyNotFoundException("Photo not found.");

            var distinctTagNames = tagNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var tags = await _unitOfWork.TagsRepository.GetTagsByNamesAsync(distinctTagNames);
            if (tags == null || !tags.Any())
                throw new KeyNotFoundException("No tags found with the provided names.");

            var assignedTagNames = photo.PhotoTags
                .Select(pt => pt.Tag.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var tag in tags)
            {
                if (!assignedTagNames.Contains(tag.Name))
                {
                    photo.PhotoTags.Add(new PhotoTag
                    {
                        Photo = photo,
                        Tag = tag,
                        CreatedBy = username,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _unitOfWork.Complete();
        }
        public async Task<List<TagDto>> GetTagsAsync(int photoId)
        {
            try
            {
                var photo = await _unitOfWork.PhotosRepository.GetPhotoWithTagsByIdAsync(photoId);
                if (photo == null) throw new KeyNotFoundException("Photo not found.");

                var tags = photo.PhotoTags.Select(pt => pt.Tag).ToList();
                return _mapper.Map<List<TagDto>>(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching tags for photo {photoId}", photoId);
                throw;
            }
        }
        public async Task<List<PhotoDto>> GetPhotoWithTagsByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required.");

            var photos = await _unitOfWork.PhotosRepository.GetPhotosByUsernameAsync(username);
            if (photos == null)
                throw new KeyNotFoundException("No photos found for this user.");
            return _mapper.Map<List<PhotoDto>>(photos);
        }
        public async Task<IEnumerable<object>> GetTagsAsync()
        {
            try
            {
                return await _unitOfWork.PhotosRepository.GetTagsAsStrings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching tags.");
                throw;
            }
        }
    }
}