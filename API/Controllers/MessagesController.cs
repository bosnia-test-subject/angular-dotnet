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
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        try
        {
            var username = User.GetUsername();
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
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        try
        {
            messageParams.Username = User.GetUsername();
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
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        try
        {
            var currentUsername = User.GetUsername();
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
    public async Task<ActionResult> DeleteMessage(int id)
    {
        try
        {
            var username = User.GetUsername();
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