using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
    Task<IEnumerable<TagDto>> GetTags();
    void AddTag(Tag tag);
    Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos();
    Task<Photo?> GetPhotoById(int id);
    Task<Photo?> GetPhotoWithTagsByIdAsync(int id);
    Task<List<Photo>> GetPhotosByUsernameAsync(string username);
    Task<List<PhotoStatsDto>> GetPhotoApprovalStatsAsync(int currentUserId);
    Task<List<string>> GetUsersWithoutMainPhotoAsync(int currentUserId);
    void RemovePhoto(Photo photo);
}
