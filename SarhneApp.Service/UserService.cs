using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SarhneApp.Core;
using SarhneApp.Core.DTOs;
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
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private ApiResponse _response;
        public UserService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager) {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _response = new();
        }

        #region Get User By Link
        public async Task<ApiResponse> GetUserByLinkAsync(string link)
        {
            if (String.IsNullOrEmpty(link))
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("invalid link");
                return _response;
            }

            var user = await _unitOfWork.Repositry<User>().GetAsync(u => u.Link == link);
            if (user == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("user NotFound");
                return _response;
            }

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<UserDto>(user);

            return _response;
        }

        #endregion

        #region Update User Data
        public async Task<ApiResponse> UpdateUserDataAsync(string userId, UpdateUserDataDto updateUserDataDto)
        {
            // Fetch the user from the repository using the userId, and track changes.
            var user = await _unitOfWork.Repositry<User>().GetAsync(
                u => u.Id == userId,
                tracked: true
            );

            // If the user is not found, return a NotFound response.
            if (user == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("User not found.");
                return _response;
            }

            // Update user properties with the data received in the DTO.
            user.Gender = updateUserDataDto.Gender;
            user.Name = updateUserDataDto.Name;
            user.DetailsAboutMe = updateUserDataDto.DetailsAboutMe;

            // Save changes to the database.
            await _unitOfWork.CompleteAsync();

            // Prepare the success response, including the updated user data.
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<UserDto>(user);

            // Return the response object.
            return _response;
        }

        #endregion

        #region Update Email
        public async Task<ApiResponse> UpdateEmailAsync(string userId, UpdateUserEmailDto updateEmailDto)
        {
            // Fetch the user from the Identity framework.
            var user = await _userManager.FindByIdAsync(userId);

            // If the user is not found, return a NotFound response.
            if (user == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("User not found.");
                return _response;
            }

            // Update the user's email and generate a confirmation token.
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, updateEmailDto.NewEmail);
            var result = await _userManager.ChangeEmailAsync(user, updateEmailDto.NewEmail, token);

            // If the update failed, return a BadRequest response with the errors.
            if (!result.Succeeded)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("invalid changing email");
                return _response;
            }

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<UserDto>(user);

            return _response;
        }
        #endregion

        #region Update Password
        public async Task<ApiResponse> UpdatePasswordAsync(string userId, UpdateUserPasswordDto updatePasswordDto)
        {
            // Fetch the user from the Identity framework.
            var user = await _userManager.FindByIdAsync(userId);

            // If the user is not found, return a NotFound response.
            if (user == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("User not found.");
                return _response;
            }

            // Attempt to change the user's password.
            var result = await _userManager.ChangePasswordAsync(user, updatePasswordDto.CurrentPassword, updatePasswordDto.NewPassword);

            // If the update failed, return a BadRequest response with the errors.
            if (!result.Succeeded)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("invalid changing password!");
                return _response;
            }

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<UserDto>(user);

            return _response;
        }

        #endregion

        #region Change User Link
        public async Task<ApiResponse> ChangeUserLinkAsync(string userId, UpdateUserLinkDto updateUserLinkDto)
        {
            // Fetch the user by ID from the database.
            var user = await _userManager.FindByIdAsync(userId);

            // Check if the user exists.
            if (user == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("User not found.");
                return _response;
            }

            // Check if the new link is unique.
            var existingUserWithSameLink = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Link == updateUserLinkDto.Link && u.Id != userId);

            if (existingUserWithSameLink != null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Conflict;
                _response.ErrorMessages.Add("This link is already taken. Please choose another one.");
                return _response;
            }

            // Update the user's link.
            user.Link = updateUserLinkDto.Link;

            // Save the changes to the database.
            var result = await _userManager.UpdateAsync(user);

            // If the update failed, return a BadRequest response with the errors.
            if (!result.Succeeded)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Invalid changing link.");
                return _response;
            }

            // Return a successful response.
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<UserDto>(user);

            return _response;
        }

        #endregion   
    }
}
