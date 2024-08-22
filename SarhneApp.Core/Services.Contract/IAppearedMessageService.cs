using SarhneApp.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.Services.Contract
{
    public interface IAppearedMessageService
    {
        Task<ApiResponse> AddReplyAppearedMessageAsync(AddReplyAppearedMessageDto dto, string userId);
        Task<ApiResponse> DeteteReplyAppearedMessageAsync(int ReplyAppearedMessageId, string userId);
        Task<ApiResponse> UpdateReplyAppearedMessageAsync(int ReplyAppearedMessageId, string userId, UpdateReplyAppearedMessageDto dto);
    }
}
