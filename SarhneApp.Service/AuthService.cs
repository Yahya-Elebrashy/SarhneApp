using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SarhneApp.Core;
using SarhneApp.Core.DTOs;
using SarhneApp.Core.Entities;
using SarhneApp.Core.Identity;
using SarhneApp.Core.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IFileService _fileService;
        private readonly ApiResponse _response;
        public AuthService(UserManager<User> userManager, IFileService fileService, ITokenService tokenService,
            SignInManager<User> signInManager,IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _response = new();
            _fileService = fileService;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
        }

        #region Register method
        public async Task<ApiResponse> RegisterAsync(RegisterDto registerDto)
        {

            // Check if a user with the provided email already exists
            var existingUserByEmail = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUserByEmail != null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("User already exists with this email");
                return _response;
            }

            // Check if a user with the provided username already exists
            var existingUserByUsername = await _userManager.FindByNameAsync(registerDto.UserName);
            if (existingUserByUsername != null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Username is already taken");
                return _response;
            }

            // Check if a user with the provided link already exists
            var userWithLink = await _unitOfWork.Repositry<User>().GetAsync(u => u.Link == registerDto.Link);
            if (userWithLink != null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Invalid link");
                return _response;
            }

            // Generate a unique file name for the user image
            string fileName = _fileService.GenerateFileName(registerDto.Image.FileName);

            // Create a new user object with the provided details
            var user = new User()
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                Gender = registerDto.Gender,
                Name = registerDto.Name,
                Link = registerDto.Link ?? registerDto.UserName,
                DetailsAboutMe = registerDto.DetaisAboutMe,
                ImageUrl = "/UserImage/" + fileName
            };

            // Create the user in the system
            var userCreated = await _userManager.CreateAsync(user, registerDto.Password);
            if (userCreated.Succeeded)
            {
                // Save the user image to the server
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserImage", fileName);
                await _fileService.SaveImageAsync(registerDto.Image, filePath);

                // Prepare a successful response with user details
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.Created;
                _response.Result = new UserResponseDto
                {
                    Name = user.Name,
                    Email = user.Email,
                    UserName = user.UserName
                };
                return _response;
            }

            // If user creation failed, return a response with error messages
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.ErrorMessages.AddRange(userCreated.Errors.Select(e => e.Description));
            return _response;
        } 
        #endregion

        #region LoginMethod
        public async Task<ApiResponse> LoginAsync(LoginDto loginDto)
        {
            // Attempt to find the user by email
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                // User not found, return an error response
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("User is Not Exist");
                return _response;
            }

            // Check if the provided password is correct
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                // Authentication failed, return an error response
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.ErrorMessages.Add("Unauthorized access. Invalid credentials.");
                return _response;
            }

            // Generate a new refresh token
            var refreshToken = _tokenService.GenerateRefreshToken();
            // Add the refresh token to the user's list of tokens
            user.RefreshTokens?.Add(refreshToken);
            // Update the user with the new refresh token
            await _userManager.UpdateAsync(user);

            // Prepare a successful response with user details and tokens
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = new UserLoginDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.UserName,
                Token = await _tokenService.CreateToken(user, _userManager),
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiresOn
            };

            return _response;

        }
        #endregion

        public async Task<ApiResponse> RefreshTokenAsync(string refreshToken)
        {
            // Find the user with the provided refresh token
            var user = await _unitOfWork.Repositry<User>()
                .GetAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));

            // Validate the refresh token and user
            if (user == null || !_tokenService.ValidateRefreshToken(user, refreshToken))
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.ErrorMessages.Add("Invalid refresh token.");
                return _response;
            }

            // Revoke the existing refresh token
            var storedRefreshToken = user.RefreshTokens.First(rt => rt.Token == refreshToken);
            storedRefreshToken.RevokedOn = DateTime.UtcNow;

            // Generate a new refresh token and add it to the user's list
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            // Generate a new JWT token
            var newJwtToken = await _tokenService.CreateToken(user, _userManager);

            // Prepare the response with the new tokens and user information
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = new UserLoginDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.UserName,
                Token = newJwtToken,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpiration = newRefreshToken.ExpiresOn
            };

            return _response;
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            // Find the user with the provided refresh token
            var user = await _unitOfWork.Repositry<User>()
                .GetAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));

            // Validate the refresh token and user
            if (user == null || !_tokenService.ValidateRefreshToken(user, refreshToken))
            {
                return false;
            }

            // Revoke the existing refresh token
            var storedRefreshToken = user.RefreshTokens.First(rt => rt.Token == refreshToken);
            storedRefreshToken.RevokedOn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return true;
        }
    }
}
