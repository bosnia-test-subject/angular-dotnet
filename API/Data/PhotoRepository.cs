using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository(DataContext context, IMapper mapper) : IPhotoRepository
{
    public async Task<Photo?> GetPhotoById(int id)
    {
        return await context.Photos
        .IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
    }

    // public async Task<IEnumerable<TagDto>> GetTags()
    // {
    //     var query = context.Photos
    //         .IgnoreQueryFilters()
    //         .SelectMany(x => x.PhotoTags.Select(pt => pt.Tag))
    //         .Distinct()
    //         .AsQueryable();

    //     return await query.ProjectTo<TagDto>(mapper.ConfigurationProvider).ToListAsync();
    // }
    public async Task<IEnumerable<TagDto>> GetTags()
    {
        var query = context.Tags.AsQueryable();
        return await query.ProjectTo<TagDto>(mapper.ConfigurationProvider).ToListAsync();
    }
    public void AddTag(Tag tag)
    {
        context.Tags.Add(tag);
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
