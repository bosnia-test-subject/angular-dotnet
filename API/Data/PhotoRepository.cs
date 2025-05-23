using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

// PHOTO MANAGEMENT TASK
public class PhotoRepository(DataContext context, IMapper mapper) : IPhotoRepository
{
    public async Task<Photo?> GetPhotoById(int id)
    {
        return await context.Photos
        .IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
    {
        var query = context.Photos.IgnoreQueryFilters().Where(x => x.isApproved == false).AsQueryable();
        return await query.ProjectTo<PhotoForApprovalDto>(mapper.ConfigurationProvider).ToListAsync();
    }
    public void RemovePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }
}
