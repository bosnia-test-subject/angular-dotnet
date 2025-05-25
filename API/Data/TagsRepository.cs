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

            return await _context.Tags
                .Where(t => names.Contains(t.Name))
                .ToListAsync();
        }
    }
}