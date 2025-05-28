using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class TagsRepository : ITagsRepository
    {
        private readonly DataContext _context;
        public TagsRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<List<Tag>> GetTagsByNamesAsync(List<string> names)
        {
            if (names == null || !names.Any())
                return new List<Tag>();

            var distinctNames = names
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var allTags = await _context.Tags.ToListAsync();

            var matchingTags = allTags
                .Where(t => distinctNames.Contains(t.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var existingNames = matchingTags
                .Select(t => t.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var name in distinctNames)
            {
                if (!existingNames.Contains(name))
                {
                    matchingTags.Add(new Tag { Name = name });
                    existingNames.Add(name);
                }
            }
            return matchingTags;
        }
        public async Task<Tag?> GetTagByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            var loweredName = name.ToLower();
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == loweredName);
        }
        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _context.Tags.ToListAsync();
        }
        public async Task RemoveTagByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            var loweredName = name.ToLower();
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == loweredName);
            if (tag != null)
            {
                _context.Tags.Remove(tag);
            }
        }
        public async Task RemoveTagFromPhotoAsync(int photoId, string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName)) return;

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());
            if (tag == null) return;

            var photoTag = await _context.PhotoTags
                .FirstOrDefaultAsync(pt => pt.PhotoId == photoId && pt.TagId == tag.Id);
            if (photoTag != null)
            {
                _context.PhotoTags.Remove(photoTag);
            }
        }
    }
}