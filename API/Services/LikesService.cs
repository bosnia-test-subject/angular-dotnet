using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;

namespace API.Services
{
    public class LikesService : ILikesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LikesService> _logger;

        public LikesService(IUnitOfWork unitOfWork, ILogger<LikesService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ToggleLikeAsync(int sourceUserId, int targetUserId)
        {
            try
            {
                if (sourceUserId == targetUserId)
                    throw new InvalidOperationException("You cannot like yourself!");

                var existingLike = await _unitOfWork.LikesRepository.GetUserLike(sourceUserId, targetUserId);

                if (existingLike == null)
                {
                    var like = new UserLike
                    {
                        SourceUserId = sourceUserId,
                        TargetUserId = targetUserId
                    };

                    _unitOfWork.LikesRepository.AddLike(like);
                }
                else
                {
                    _unitOfWork.LikesRepository.DeleteLike(existingLike);
                }

                if (!await _unitOfWork.Complete())
                    throw new Exception("Failed to update like.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling like for targetUserId: {targetUserId}", targetUserId);
                throw;
            }
        }

        public async Task<IEnumerable<int>> GetCurrentUserLikeIdsAsync(int userId)
        {
            try
            {
                return await _unitOfWork.LikesRepository.GetCurrentUserIds(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching current user like IDs for userId: {userId}", userId);
                throw;
            }
        }

        public async Task<PagedList<MemberDto>> GetUserLikesAsync(LikesParams likesParams)
        {
            try
            {
                return await _unitOfWork.LikesRepository.GetUserLikes(likesParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user likes.");
                throw;
            }
        }
    }
}