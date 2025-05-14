using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserService
    {
        Task<PagedList<MemberDto>> GetUsersAsync(UserParams userParams);
        Task<MemberDto> GetUserAsync(string username);
        Task UpdateUserAsync(string username, MemberUpdateDto memberUpdateDto);
        Task<PhotoDto> AddPhotoAsync(string username, IFormFile file);
        Task SetMainPhotoAsync(string username, int photoId);
        Task DeletePhotoAsync(string username, int photoId);
    }
}