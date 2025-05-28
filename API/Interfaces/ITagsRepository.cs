using API.Entities;

namespace API.Interfaces
{
    public interface ITagsRepository
    {
        Task<Tag?> GetTagByNameAsync(string name);
        Task<List<Tag>> GetTagsByNamesAsync(List<string> tags);
        Task<List<Tag>> GetAllTagsAsync();
        Task RemoveTagByName(string name);
    }
}