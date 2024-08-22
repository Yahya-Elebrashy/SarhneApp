using AutoMapper;
using SarhneApp.Core;
using SarhneApp.Core.DTOs;
using SarhneApp.Core.Entities;
using SarhneApp.Core.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Service
{
    public class AppearedMessageService : IAppearedMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private ApiResponse _response;

        public AppearedMessageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _response = new();
        }
        #region Add Reply Appeared Message
        public async Task<ApiResponse> AddReplyAppearedMessageAsync(AddReplyAppearedMessageDto dto, string userId)
        {
            // Fetch non-deleted message for the user
            var message = await _unitOfWork.Repositry<Message>()
                .GetAsync(
                    m => m.Id == dto.MessageId && m.ReceiverId == userId && !m.IsDeleted,
                    includeProperties: "Sender,Receiver"
                );
            if (message == null)
            {
                // Message not found or doesn't belong to the user
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("Message not found or access denied");
                return _response;
            }

            if (!message.IsAppeared)
            {
                // Message not found or doesn't belong to the user
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Message must be appeared");
                return _response;
            }

            var replyAppearedMessageMapped = _mapper.Map<ReplyAppearedMessage>(dto);

            await _unitOfWork.Repositry<ReplyAppearedMessage>()
                .AddAsync(replyAppearedMessageMapped);

            var saveChangesResult = await _unitOfWork.CompleteAsync();
            if (saveChangesResult <= 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages.Add("Error sending replay.");
                return _response;
            }

            if (message.IsSecretly)
            {
                message.Sender = null;
            }
            replyAppearedMessageMapped.Message = message;

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = _mapper.Map<ReplyAppearedMessageDto>(replyAppearedMessageMapped);
            return _response;
        }

        #endregion

        #region Delete Reply Appeared Message
        public async Task<ApiResponse> DeteteReplyAppearedMessageAsync(int ReplyAppearedMessageId, string userId)
        {
            // Fetch reply appeared message for the user
            var replyAppearedMessage = await _unitOfWork.Repositry<ReplyAppearedMessage>()
                .GetAsync(
                    m => m.Id == ReplyAppearedMessageId && m.Message.ReceiverId == userId
                );
            if (replyAppearedMessage == null)
            {
                // Message not found or doesn't belong to the user
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("Message not found or access denied");
                return _response;
            }

            await _unitOfWork.Repositry<ReplyAppearedMessage>()
                .DeleteAsync(replyAppearedMessage);
            var saveChangesResult = await _unitOfWork.CompleteAsync();
            if (saveChangesResult <= 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages.Add("Error Deleting replay.");
                return _response;
            }
            _response.StatusCode = HttpStatusCode.NoContent;
            return _response;
        }
        #endregion

        #region Update Reply Appeared Message
        public async Task<ApiResponse> UpdateReplyAppearedMessageAsync(int ReplyAppearedMessageId, string userId, UpdateReplyAppearedMessageDto dto)
        {
            // Fetch reply appeared message for the user
            var replyAppearedMessage = await _unitOfWork.Repositry<ReplyAppearedMessage>()
                .GetAsync(
                    m => m.Id == ReplyAppearedMessageId && m.Message.ReceiverId == userId,
                    tracked: true,
                    includeProperties: "Message"
                );
            if (replyAppearedMessage == null)
            {
                // Message not found or doesn't belong to the user
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("Message not found or access denied");
                return _response;
            }

            replyAppearedMessage.ReplyText = dto.ReplyText;
            await _unitOfWork.CompleteAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = _mapper.Map<ReplyAppearedMessageDto>(replyAppearedMessage);
            return _response;
        }

        #endregion    }
    }
}
