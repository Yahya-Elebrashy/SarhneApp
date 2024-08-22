using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using SarhneApp.Core.DTOs;
using SarhneApp.Core.Entities;
using SarhneApp.Core.Services.Contract;
using System.Net;
using System.Security.Claims;

namespace SarhneApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ApiResponse _response;
        private readonly IMessageService _messageService;
        private readonly IMapper _mapper;
        public MessageController(IMessageService messageService, IMapper mapper)
        {
            _response = new();
            _messageService = messageService;
            _mapper = mapper;
        }
        #region Send Message
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> SendMessage(SendMessageDto dto)
        {
            string? senderId = User.Identity.IsAuthenticated
                      ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      : null;

            var isAuthenticated = User.Identity.IsAuthenticated;
            var messageSent = await _messageService.SendMessageAsync(dto, senderId, isAuthenticated);
            return StatusCode((int)messageSent.StatusCode, messageSent);
        }
        #endregion

        #region Get Receive Messagesa
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("received-messages")]
        public async Task<ActionResult<ApiResponse>> GetReceivedMessages()
        {
            // Get the authenticated user Id from the claims
            string? userId = User.Identity.IsAuthenticated
                ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                : null;

            // Call the service method to retrieve the received messages for the user
            var response = await _messageService.GetReceivedMessagesAsync(userId);

            // Return the response with the appropriate status code
            return StatusCode((int)response.StatusCode, response);
        }
        #endregion

        #region Get Sent Messagesa
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("sent-messages")]
        public async Task<ActionResult<ApiResponse>> GetSentMessages()
        {
            // Get the authenticated user's ID from the claims
            string? userId =  User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Call the service method to retrieve the sent messages for the user
            var response = await _messageService.GetSentMessagesAsync(userId);

            // Return the response with the appropriate status code
            return StatusCode((int)response.StatusCode, response);
        }
        #endregion

        #region Delete Received Message
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("received-messages/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteReceivedMessage(int id)
        {
            // Get the authenticated user Id from the claims
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Call the service method to delete the message
            var response = await _messageService.DeleteReceivedMessageAsync(id, userId);

            // Return the response with the appropriate status code
            return StatusCode((int)response.StatusCode, response);
        }
        #endregion

        #region Update Appeared Messages
        [Authorize]
        [HttpPut("appeared-messages/{id}")]
        public async Task<IActionResult> UpdateAppeared(int id, [FromBody] UpdateAppearedDto updateAppearedDto)
        {
            // Get the authenticated user Id from the claims
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Call the service method to Update IsAppeared property
            var response = await _messageService.UpdateAppearedMessageAsync(id, userId, updateAppearedDto.IsAppeared);

            // Return the response with the appropriate status code
            return StatusCode((int)response.StatusCode, response);
        }
        #endregion

        #region Update Favorite Messages
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("update-favorite/{id}")]
        public async Task<IActionResult> UpdateFavorite(int id, [FromBody] UpdateFavoriteDto updateFavoriteDto)
        {
            // Get the authenticated user Id from the claims
            string? userId = User.Identity.IsAuthenticated
                ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                : null;

            // Call the service method to update the IsFavorite property
            var response = await _messageService.UpdateFavoriteAsync(id, userId, updateFavoriteDto.IsFavorite);

            // Return the response with the appropriate status code
            return StatusCode((int)response.StatusCode, response);
        }
        #endregion

        #region Get Favorited Messages
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("favorited-messages")]
        public async Task<ActionResult<ApiResponse>> GetFavoritedMessages()
        {
            // Get the authenticated user Id from the claims
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Call the service method to retrieve the received messages for the user
            var response = await _messageService.GetFavoritedMessagesAsync(userId);

            // Return the response with the appropriate status code
            return StatusCode((int)response.StatusCode, response);
        }
        #endregion

        #region Get Appeared Messages
        [Authorize]
        [HttpGet("appeared-messages")]
        public async Task<ActionResult<ApiResponse>> GetAppearedMessages()
        {
            // Get the authenticated user Id from the claims
            string? userId = User.Identity.IsAuthenticated
                ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                : null;

            // Call the service method to retrieve the received messages for the user
            var response = await _messageService.GetAppearedMessagesAsync(userId);

            // Return the response with the appropriate status code
            return StatusCode((int)response.StatusCode, response);
        }
        #endregion

    }
}
