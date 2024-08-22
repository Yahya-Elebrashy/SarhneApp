using SarhneApp.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.Services.Contract
{
    public interface IAuthService
    {
        Task<ApiResponse> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse> LoginAsync(LoginDto loginDto);
        Task<ApiResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
    }
}
