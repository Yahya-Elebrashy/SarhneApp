using SarhneApp.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.Services.Contract
{
    public interface IUserService
    {
        Task<ApiResponse> GetUserByLinkAsync(string link);
        Task<ApiResponse> UpdateUserDataAsync(string userId, UpdateUserDataDto dto);
        Task<ApiResponse> UpdateEmailAsync(string userId, UpdateUserEmailDto updateEmailDto);
        Task<ApiResponse> UpdatePasswordAsync(string userId, UpdateUserPasswordDto updatePasswordDto);
        Task<ApiResponse> ChangeUserLinkAsync(string userId, UpdateUserLinkDto updateUserLinkDto);
    }
}
