using API.DTOs;

namespace API.Interfaces
{
    public interface IAdminService
    {
        Task<object> CreateTagAsync(string tagName);
        Task<IEnumerable<object>> GetTagsAsync();
        Task<IEnumerable<object>> GetPhotosForApprovalAsync();
        Task<List<PhotoStatsDto>> GetPhotoApprovalStatsAsync(int currentUserId);
        Task<List<string>> GetUsersWithoutMainPhotoAsync(int currentUserId);
        Task RejectPhotoAsync(int id);
        Task ApprovePhotoAsync(int id);
        Task<IEnumerable<object>> GetUsersWithRolesAsync();
        Task EditRolesAsync(string username, string roles);
    }
}