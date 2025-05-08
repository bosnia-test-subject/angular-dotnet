using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
    // PHOTO MANAGEMENT TASK
    Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos(string username);
    Task<Photo?> GetPhotoById(int id);
    void RemovePhoto(Photo photo);
}
