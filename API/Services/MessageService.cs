using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace API.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MessageService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<MessageDto> CreateMessageAsync(string username, CreateMessageDto createMessageDto)
        {
            try
            {
                if (username == createMessageDto.RecipientUsername.ToLower())
                    throw new InvalidOperationException("You cannot message yourself!");

                var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
                var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

                if (sender == null || recipient == null)
                    throw new KeyNotFoundException("Sender or recipient not found.");

                if(sender.UserName == null || recipient.UserName == null)
                    throw new KeyNotFoundException("Sender or recipient not found.");

                var message = new Message
                {
                    Sender = sender,
                    Recipient = recipient,
                    SenderUsername = sender.UserName,
                    RecipientUsername = recipient.UserName,
                    Content = createMessageDto.Content
                };

                _unitOfWork.MessageRepository.AddMessage(message);

                if (await _unitOfWork.Complete())
                    return _mapper.Map<MessageDto>(message);

                throw new Exception("Failed to save message.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a message.");
                throw;
            }
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams)
        {
            try
            {
                var messages = await _unitOfWork.MessageRepository.GetMesagesForUser(messageParams);
                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching messages for user.");
                throw;
            }
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string username)
        {
            try
            {
                return await _unitOfWork.MessageRepository.GetMessageThread(currentUsername, username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching message thread.");
                throw;
            }
        }

        public async Task DeleteMessageAsync(string username, int id)
        {
            try
            {
                var message = await _unitOfWork.MessageRepository.GetMessage(id);

                if (message == null)
                    throw new KeyNotFoundException("Message not found.");

                if (message.SenderUsername != username && message.RecipientUsername != username)
                    throw new UnauthorizedAccessException("You are not authorized to delete this message.");

                if (message.SenderUsername == username) message.SenderDeleted = true;
                if (message.RecipientUsername == username) message.RecipientDeleted = true;

                if (message.SenderDeleted && message.RecipientDeleted)
                    _unitOfWork.MessageRepository.DeleteMessage(message);

                if (!await _unitOfWork.Complete())
                    throw new Exception("Problem deleting the message.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting message.");
                throw;
            }
        }
    }
}