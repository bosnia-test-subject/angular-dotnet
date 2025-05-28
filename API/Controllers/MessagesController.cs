using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Authorize]
public class MessagesController : BaseApiController
{
    private readonly IMessageService _messageService;
    private readonly ILogger<MessagesController> _logger;
    public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
    {
        _messageService = messageService;
        _logger = logger;
    }
    [HttpPost]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        _logger.LogDebug("CreateMessage endpoint called for recipient: {RecipientUsername}", createMessageDto.RecipientUsername);

        try
        {
            var username = User.GetUsername();
            if (string.IsNullOrWhiteSpace(username) || username == null)
            {
                _logger.LogWarning("Username is missing or invalid.");
                return NotFound("Username not found.");
            }
            var message = await _messageService.CreateMessageAsync(username, createMessageDto);
            return Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating a message.");
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found while creating a message.");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating a message.");
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        _logger.LogDebug("GetMessagesForUser endpoint called with container: {Container}", messageParams.Container);

        try
        {
            var username = User.GetUsername();
            if (string.IsNullOrWhiteSpace(username) || username == null)
            {
                _logger.LogWarning("Username is missing or invalid.");
                return NotFound("Username not found.");
            }
            messageParams.Username = username;
            var messages = await _messageService.GetMessagesForUserAsync(messageParams);
            Response.AddPaginationHeader((PagedList<MessageDto>)messages);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching messages for user.");
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet("thread/{username}")]
    [ProducesResponseType(typeof(IEnumerable<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        _logger.LogDebug("GetMessageThread endpoint called for user: {Username}", username);

        try
        {
            var currentUsername = User.GetUsername();
            if (string.IsNullOrWhiteSpace(currentUsername) || currentUsername == null)
            {
                _logger.LogWarning("Username is missing or invalid.");
                return NotFound("Username not found.");
            }
            var messages = await _messageService.GetMessageThreadAsync(currentUsername, username);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching message thread.");
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        _logger.LogDebug("DeleteMessage endpoint called for messageId: {MessageId}", id);

        try
        {
            var username = User.GetUsername();
            if (string.IsNullOrWhiteSpace(username) || username == null)
            {
                _logger.LogWarning("Username is missing or invalid.");
                return NotFound("Username not found.");
            }
            await _messageService.DeleteMessageAsync(username, id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Message not found while deleting.");
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access while deleting message.");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting message.");
            return StatusCode(500, "Internal server error");
        }
    }
}