using AutoMapper;
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
    public class ReactionService : IReactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private ApiResponse _response;
        public ReactionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _response = new();
        }

        #region Get Reactions
        public async Task<ApiResponse> GetReactionsAsync()
        {
            var reactions = await _unitOfWork.Repositry<Reaction>().GetAllAsync();

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<IReadOnlyList<ReactionDto>>(reactions);

            return _response;
        }
        #endregion

        #region React To Message
        public async Task<ApiResponse> ReactToMessageAsync(AddReactToMessageDto reactToMessageDto, string userId)
        {
            var user = await _unitOfWork.Repositry<User>().GetAsync(u => u.Id == userId);
            if (user == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Invalid user.");
                return _response;
            }

            var message = await _unitOfWork.Repositry<Message>().GetAsync(m => m.Id == reactToMessageDto.MessageId);
            if (message == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Invalid message.");
                return _response;
            }

            var reaction = await _unitOfWork.Repositry<Reaction>().GetAsync(r => r.Id == reactToMessageDto.ReactionId);
            if (reaction == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Invalid reaction.");
                return _response;
            }

            // Check if the reaction already exists for this user and message
            var existingReaction = await _unitOfWork.Repositry<UserReaction>().GetAsync(ur => ur.UserId == userId
                                        && ur.MessageId == reactToMessageDto.MessageId,
                                        tracked: true,
                                        includeProperties: "Message,Reaction"
                                        );

            if (existingReaction != null)
            {
                // Update the existing reaction
                existingReaction.ReactionId = reactToMessageDto.ReactionId;
                await _unitOfWork.Repositry<UserReaction>().UpdateAsync(existingReaction);

            }
            else
            {
                // Create a new reaction
                var newReaction = new UserReaction
                {
                    UserId = userId,
                    MessageId = reactToMessageDto.MessageId,
                    ReactionId = reactToMessageDto.ReactionId
                };

                await _unitOfWork.Repositry<UserReaction>().AddAsync(newReaction);
            }

            // Save changes to the database
            await _unitOfWork.CompleteAsync();

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<ReactToMessageDto>(existingReaction);
            return _response;
        }

        #endregion    

    }
}
