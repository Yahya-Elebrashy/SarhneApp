using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SarhneApp.Core.DTOs;
using SarhneApp.Core.Services.Contract;
using System.Security.Claims;

namespace SarhneApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        #region Get User By Link
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("userByLink/{link}")]
        public async Task<ActionResult<ApiResponse>> GetUserByLink(string link)
        {
            var userByLink = await _userService.GetUserByLinkAsync(link);
            return StatusCode((int)userByLink.StatusCode, userByLink);
        }
        #endregion

        #region Update User Data 
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("updateUserData")]
        public async Task<ActionResult<ApiResponse>> UpdateUserData(UpdateUserDataDto updateUserDataDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var apiResponse = await _userService.UpdateUserDataAsync(userId, updateUserDataDto);

            return StatusCode((int)apiResponse.StatusCode, apiResponse);
        }
        #endregion

        #region Update User Email
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("updateEmail")] 
        public async Task<ActionResult<ApiResponse>> UpdateEmail(UpdateUserEmailDto updateEmailDto)
        {
            // Retrieve the user's ID from the JWT claims.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Call the service method to update the user's email.
            var apiResponse = await _userService.UpdateEmailAsync(userId, updateEmailDto);

            // Return the appropriate HTTP status code and response.
            return StatusCode((int)apiResponse.StatusCode, apiResponse);
        }
        #endregion

        #region Update User Password
        [Authorize]
        [HttpPut("updatePassword")]
        public async Task<ActionResult<ApiResponse>> UpdatePassword(UpdateUserPasswordDto updatePasswordDto)
        {
            // Retrieve the user's ID from the JWT claims.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Call the service method to update the user's password.
            var apiResponse = await _userService.UpdatePasswordAsync(userId, updatePasswordDto);

            // Return the appropriate HTTP status code and response.
            return StatusCode((int)apiResponse.StatusCode, apiResponse);
        }
        #endregion

        #region Update User Link
        [Authorize]
        [HttpPut("updateLink")] 
        public async Task<ActionResult<ApiResponse>> ChangeUserLink(UpdateUserLinkDto updateUserLinkDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var apiResponse = await _userService.ChangeUserLinkAsync(userId, updateUserLinkDto);

            return StatusCode((int)apiResponse.StatusCode, apiResponse);
        }
        #endregion


    }
}
