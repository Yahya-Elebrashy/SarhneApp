using SarhneApp.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.Services.Contract
{
    public interface IReactionService
    {
        Task<ApiResponse> GetReactionsAsync();
        Task<ApiResponse> ReactToMessageAsync(AddReactToMessageDto reactToMessageDto, string userId);
    }
}
