using Microsoft.AspNetCore.Identity;
using SarhneApp.Core.Entities;
using SarhneApp.Core.Identity;
using SarhneApp.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.Services.Contract
{
    public interface ITokenService
    {
        Task<string> CreateToken(User user, UserManager<User> userManager);
        RefreshToken GenerateRefreshToken();
        bool ValidateRefreshToken(User user, string refreshToken);
    }
}
