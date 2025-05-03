using System;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{
    void AddMessage(Message message);
    void DeleteMessage(Message message);
    Task<Message?> GetMessage(int id);
    Task<PagedList<MessageDto>> GetMesagesForUser();
    Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipient);
    Task<bool> SaveAllAsync();
}
