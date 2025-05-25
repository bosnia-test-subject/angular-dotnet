using System.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository(DataContext context, IMapper mapper) : IPhotoRepository
{
    public async Task<Photo?> GetPhotoById(int id)
    {
        return await context.Photos
        .IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
    }
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
    public async Task<Photo?> GetPhotoWithTagsByIdAsync(int id)
    {
        return await context.Photos
            .Include(p => p.PhotoTags)
            .ThenInclude(pt => pt.Tag)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    public async Task<List<PhotoStatsDto>> GetPhotoApprovalStatsAsync(int currentUserId)
    {
        var result = new List<PhotoStatsDto>();

        using var command = context.Database.GetDbConnection().CreateCommand();

        command.CommandText = "GetPhotoApprovalStats";
        command.CommandType = CommandType.StoredProcedure;

        var userIdParam = new SqlParameter("@CurrentUserId", currentUserId);
        command.Parameters.Add(userIdParam);

        await context.Database.OpenConnectionAsync();

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new PhotoStatsDto
            {
                Username = reader.GetString(0),
                ApprovedPhotos = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                UnapprovedPhotos = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
            });
        }
        return result;
    }
    public async Task<List<string>> GetUsersWithoutMainPhotoAsync(int currentUserId)
    {
        var result = new List<string>();

        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "GetUsersWithoutMainPhoto";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@CurrentUserId", currentUserId));

        await context.Database.OpenConnectionAsync();

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(reader.GetString(0));
        }
        return result;
    }
}
