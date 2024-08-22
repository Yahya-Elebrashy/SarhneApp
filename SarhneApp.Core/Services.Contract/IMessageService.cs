using SarhneApp.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.Services.Contract
{
    public interface IMessageService
    {
        Task<ApiResponse> SendMessageAsync(SendMessageDto messageDto, string senderId, bool isAuthenticated);
        Task<ApiResponse> GetReceivedMessagesAsync(string userId);
        Task<ApiResponse> GetSentMessagesAsync(string userId);
        Task<ApiResponse> DeleteReceivedMessageAsync(int messageId, string userId);
        Task<ApiResponse> UpdateAppearedMessageAsync(int messageId, string userId, bool isAppeared);
        Task<ApiResponse> UpdateFavoriteAsync(int messageId, string userId, bool isFavorite);
        Task<ApiResponse> GetFavoritedMessagesAsync(string userId);
        Task<ApiResponse> GetAppearedMessagesAsync(string userId);
    }
}
