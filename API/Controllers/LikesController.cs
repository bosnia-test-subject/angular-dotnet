using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
public class LikesController : BaseApiController
{
    private readonly ILikesService _likesService;
    private readonly ILogger<LikesController> _logger;
    public LikesController(ILikesService likesService, ILogger<LikesController> logger)
    {
        _likesService = likesService;
        _logger = logger;
    }
    [HttpPost("{targetUserId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        _logger.LogDebug("ToggleLike endpoint called for targetUserId: {TargetUserId}", targetUserId);

        try
        {
            var sourceUserId = User.GetUserId();
            await _likesService.ToggleLikeAsync(sourceUserId, targetUserId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while toggling like for targetUserId: {TargetUserId}", targetUserId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while toggling like for targetUserId: {TargetUserId}", targetUserId);
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet("list")]
    [ProducesResponseType(typeof(IEnumerable<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
        _logger.LogDebug("GetCurrentUserLikeIds endpoint called.");

        try
        {
            var userId = User.GetUserId();
            var likeIds = await _likesService.GetCurrentUserLikeIdsAsync(userId);
            return Ok(likeIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching current user like IDs.");
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
    {
        _logger.LogDebug("GetUserLikes endpoint called with predicate: {Predicate}", likesParams.Predicate);

        try
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesService.GetUserLikesAsync(likesParams);
            Response.AddPaginationHeader(users);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserLikesAsync threw an exception for Predicate: {Predicate}", likesParams.Predicate);
            return StatusCode(500, "Error retrieving likes data");
        }
    }
}