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
    void RemovePhoto(Photo photo);
}
