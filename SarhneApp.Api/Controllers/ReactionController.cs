using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SarhneApp.Core.DTOs;
using SarhneApp.Core.Entities;
using SarhneApp.Core.Services.Contract;
using System.Security.Claims;

namespace SarhneApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReactionController : ControllerBase
    {
        private readonly IReactionService _reactionService;

        public ReactionController(IReactionService reactionService)
        {
            _reactionService = reactionService;
        }
        #region GetReactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reaction>>> GetReactions()
        {
            var reactions = await _reactionService.GetReactionsAsync();
            return Ok(reactions);
        }
        #endregion

        #region ReactToMessage
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        [HttpPost("ReactToMessage")]
        public async Task<IActionResult> ReactToMessage([FromBody] AddReactToMessageDto reactToMessageDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var response = await _reactionService.ReactToMessageAsync(reactToMessageDto, userId);
            return StatusCode((int)response.StatusCode, response);
        } 
        #endregion
    }
}
