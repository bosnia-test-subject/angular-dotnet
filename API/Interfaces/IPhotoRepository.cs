using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
    // PHOTO MANAGEMENT TASK
    Task<IEnumerable<Photo>> GetUnapprovedPhotos(string username);
    Task<Photo?> GetPhotoById(int id);
    void RemovePhoto(int photoId);
}
