using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SarhneApp.Core.DTOs;
using SarhneApp.Core.Services.Contract;
using SarhneApp.Service;
using System.Security.Claims;

namespace SarhneApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReplyAppearedMessageController : ControllerBase
    {
        private readonly IAppearedMessageService _appearedMessageService;

        public ReplyAppearedMessageController(IAppearedMessageService appearedMessageService)
        {
            _appearedMessageService = appearedMessageService;
        }
        #region Add Reply Appeared Message
        [Authorize]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddReplyAppearedMessage(AddReplyAppearedMessageDto dto)
        {
            string? userId = User.Identity.IsAuthenticated
                      ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       : null;

            var messageReplyed = await _appearedMessageService.AddReplyAppearedMessageAsync(dto, userId);
            return StatusCode((int)messageReplyed.StatusCode, messageReplyed);
        }
        #endregion

        #region Delete Reply Appeared Message
        [Authorize]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpDelete("{replyAppearedMessageId}")]
        public async Task<ActionResult<ApiResponse>> DeleteReplyAppearedMessage(int replyAppearedMessageId)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var replyAppearedMessageDeleted = await _appearedMessageService.DeteteReplyAppearedMessageAsync(replyAppearedMessageId, userId);
            return StatusCode((int)replyAppearedMessageDeleted.StatusCode, replyAppearedMessageDeleted);
        }
        #endregion

        #region Update Reply Appeared Message
        [Authorize]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPut("{replyAppearedMessageId}")]
        public async Task<ActionResult<ApiResponse>> UpdateReplyAppearedMessage(int replyAppearedMessageId, UpdateReplyAppearedMessageDto dto)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var replyAppearedMessageUpdated = await _appearedMessageService.UpdateReplyAppearedMessageAsync(replyAppearedMessageId, userId, dto);
            return StatusCode((int)replyAppearedMessageUpdated.StatusCode, replyAppearedMessageUpdated);
        }
        #endregion


    }
}
