using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;

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
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Sender username must be provided.", nameof(username));

            if (createMessageDto == null)
                throw new ArgumentNullException(nameof(createMessageDto), "Message data must be provided.");

            if (string.IsNullOrWhiteSpace(createMessageDto.RecipientUsername))
                throw new ArgumentException("Recipient username must be provided.", nameof(createMessageDto.RecipientUsername));

            if (string.IsNullOrWhiteSpace(createMessageDto.Content))
                throw new ArgumentException("Message content must be provided.", nameof(createMessageDto.Content));

            if (username.Equals(createMessageDto.RecipientUsername, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("You cannot message yourself!");

            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username.Trim());
            if (sender == null || string.IsNullOrWhiteSpace(sender.UserName))
                throw new KeyNotFoundException("Sender not found or sender username is invalid.");

            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername.Trim());
            if (recipient == null || string.IsNullOrWhiteSpace(recipient.UserName))
                throw new KeyNotFoundException("Recipient not found or recipient username is invalid.");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content.Trim()
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