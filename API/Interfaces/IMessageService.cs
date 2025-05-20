using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface IMessageService
    {
        Task<MessageDto> CreateMessageAsync(string username, CreateMessageDto createMessageDto);
        Task<IEnumerable<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string username);
        Task DeleteMessageAsync(string username, int id); 
    }
}