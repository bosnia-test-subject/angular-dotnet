using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

// PHOTO MANAGEMENT TASK
public class PhotoRepository(DataContext context) : IPhotoRepository
{
    public async Task<Photo?> GetPhotoById(int id)
    {
        return await context.Photos.FindAsync(id);
    }

    public async Task<IEnumerable<Photo>> GetUnapprovedPhotos(string username)
    {
        return await context.Photos.Where(x => x.AppUser.UserName == username 
        && x.isApproved == false).ToListAsync();
    }

    public void RemovePhoto(int photoId)
    {
        var photo = context.Photos.Find(photoId);
        if (photo == null) throw new Exception("Photo id cannot be null!");
        context.Photos.Remove(photo);
    }
}
