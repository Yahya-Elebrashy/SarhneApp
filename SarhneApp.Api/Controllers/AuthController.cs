
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SarhneApp.Core.DTOs;
using SarhneApp.Core.Entities;
using SarhneApp.Core.Services.Contract;
using System.Net;

namespace SarhneApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApiResponse _response;
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _response = new();
            _authService = authService;
        }
        #region Register endpoint

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("Register")]
        public async Task<ActionResult<ApiResponse>> Register([FromForm] RegisterDto dto)
        {
            var registerd = await _authService.RegisterAsync(dto);

            return StatusCode((int)registerd.StatusCode, registerd);
        }
        #endregion

        #region Login endpoitn

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("Login")]
        public async Task<ActionResult<ApiResponse>> Login( LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(_response);

            }
            var login = await _authService.LoginAsync(loginDto);
            return StatusCode((int)login.StatusCode, login);
        }
        #endregion

        #region Refresh Token
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        [HttpPost("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequest)
        {

            // Call the AuthService to refresh the token
            var response = await _authService.RefreshTokenAsync(refreshTokenRequest.refreshToken);

            // Check if the refresh token process was unsuccessful
            if (!response.IsSuccess)
            {
                return Unauthorized(response);
            }

            // Return the new tokens if successful
            return Ok(response);
        }
        #endregion

        #region Revoke Token
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto refreshTokenRequest)
        {

            var response = await _authService.RevokeTokenAsync(refreshTokenRequest.refreshToken);

            if (!response)
            {
                return BadRequest("invalid token");
            }

            return Ok();
        }
        #endregion
    }
}
