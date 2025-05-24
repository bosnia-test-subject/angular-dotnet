using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;

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
                if (users == null)
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
    }
}