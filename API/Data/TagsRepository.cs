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
        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _context.Tags.ToListAsync();
        }
    }
}