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
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly ApiResponse _response;
        public MessageService(IUnitOfWork unitOfWork, IFileService fileService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _mapper = mapper;
            _response = new();
        }

        #region Get Received Messages
        public async Task<ApiResponse> GetReceivedMessagesAsync(string userId)
        {
            // Fetch all non-deleted messages for the user, including sender, receiver, appeared replies, and user reactions details
            var messages = await _unitOfWork.Repositry<Message>()
                .GetAllAsync(
                    m => m.ReceiverId == userId && !m.IsDeleted,
                    false,
                    includeProperties: "Sender,Receiver,AppearedReplies,UserReactions.Reaction"
                );

            // List to hold the mapped MessageDto objects
            var messageDtos = messages.Select(message =>
            {
                // If the message is marked as "secret", hide the sender information
                if (message.IsSecretly)
                {
                    message.Sender = null;
                }

                // Calculate reaction counts grouped by reaction type
                var reactionCounts = message.UserReactions
                    .GroupBy(ur => ur.Reaction.ReactionType)
                    .Select(group => new ReactionCountDto
                    {
                        ReactionType = group.Key,
                        Count = group.Count()
                    })
                    .ToList();

                // Map the message to its DTO and include reaction counts
                var messageDto = _mapper.Map<MessageDto>(message);
                messageDto.ReactionCounts = reactionCounts;

                return messageDto;
            }).ToList();

            // Set the success status and assign the mapped messages to the response
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = messageDtos;

            // Return the response object
            return _response;
        }
        #endregion

        #region Get Sent Messages
        public async Task<ApiResponse> GetSentMessagesAsync(string userId)
        {
            // Fetch all messages sent by the user, including sender and receiver details
            var messages = await _unitOfWork.Repositry<Message>()
                .GetAllAsync(
                    m => m.SenderId == userId,
                    false,
                    includeProperties: "Sender,Receiver,AppearedReplies,UserReactions.Reaction"
                );

            // List to hold the mapped MessageDto objects
            var messageDtos = messages.Select(message =>
            {
                // Calculate reaction counts grouped by reaction type
                var reactionCounts = message.UserReactions
                    .GroupBy(ur => ur.Reaction.ReactionType)
                    .Select(group => new ReactionCountDto
                    {
                        ReactionType = group.Key,
                        Count = group.Count()
                    })
                    .ToList();

                // Map the message to its DTO and include reaction counts
                var messageDto = _mapper.Map<MessageDto>(message);
                messageDto.ReactionCounts = reactionCounts;

                return messageDto;
            }).ToList();

            // Set the success status and map the messages to DTOs
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = messageDtos;

            // Return the response object
            return _response;
        }

        #endregion

        #region Delete Received Message
        public async Task<ApiResponse> DeleteReceivedMessageAsync(int messageId, string userId)
        {
            // Find the message by its ID and ensure it belongs to the user
            var message = await _unitOfWork.Repositry<Message>()
                .GetAsync(
                    m => m.Id == messageId && m.ReceiverId == userId && !m.IsDeleted,
                    tracked: true,
                    includeProperties: "AppearedReplies"
                );

            if (message == null)
            {
                // Message not found or doesn't belong to the user
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("Message not found or access denied");
                return _response;
            }

            // Mark the message as deleted
            message.IsDeleted = true;
            await _unitOfWork.Repositry<ReplyAppearedMessage>()
                .DeleteRangeAsync(message.AppearedReplies);
            // Save changes to the database
            var saveChangesResult = await _unitOfWork.CompleteAsync();
            if (saveChangesResult <= 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages.Add("Error deleting message.");
                return _response;
            }
            // Set the success status
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.NoContent;

            // Return the response object
            return _response;
        }

        #endregion

        #region Send Message
        public async Task<ApiResponse> SendMessageAsync(SendMessageDto messageDto, string senderId, bool isAuthenticated)
        {
            if (string.IsNullOrEmpty(messageDto.MessageText) && messageDto.Image == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Image and message text cannot be empty.");
                return _response;
            }

            if (!isAuthenticated && messageDto.IsSecretly == false)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.ErrorMessages.Add("User is not authenticated.");
                return _response;
            }

            var messageMapped = _mapper.Map<Message>(messageDto);

            if (senderId != null)
            {
                messageMapped.SenderId = senderId;
            }

            string fileName = null;
            if (messageDto.Image != null)
            {
                fileName = _fileService.GenerateFileName(messageDto.Image.FileName);
                messageMapped.ImageUrl = "/MessagesImage/" + fileName;
            }
            await _unitOfWork.Repositry<Message>().AddAsync(messageMapped);
            var saveChangesResult = await _unitOfWork.CompleteAsync();
            if (saveChangesResult <= 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages.Add("Error sending message.");
                return _response;
            }
            if (!String.IsNullOrEmpty(fileName))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/MessagesImage", fileName);
                await _fileService.SaveImageAsync(messageDto.Image, filePath);
            }
            var createdMessage = await _unitOfWork.Repositry<Message>().GetAsync(m => m.Id == messageMapped.Id, false, "Sender,Receiver,UserReactions.Reaction");
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = _mapper.Map<MessageDto>(createdMessage);
            return _response;
        }

        #endregion

        #region Update Appeared Message
        public async Task<ApiResponse> UpdateAppearedMessageAsync(int messageId, string userId, bool isAppeared)
        {
            // Find the message by its ID and ensure it belongs to the user
            var message = await _unitOfWork.Repositry<Message>()
                .GetAsync(
                    m => m.Id == messageId && m.ReceiverId == userId && !m.IsDeleted, tracked: true
                );

            if (message == null)
            {
                // Message not found or doesn't belong to the user
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("Message not found or access denied");
                return _response;
            }

            // Mark the message as deleted
            message.IsAppeared = isAppeared;

            // Save changes to the database
            await _unitOfWork.CompleteAsync();

            // Set the success status
            _response.StatusCode = HttpStatusCode.NoContent;

            // Return the response object
            return _response;
        }
        #endregion

        #region Update Favorite Message
        public async Task<ApiResponse> UpdateFavoriteAsync(int messageId, string userId, bool isFavorite)
        {
            // Find the message by its ID and ensure it belongs to the user
            var message = await _unitOfWork.Repositry<Message>()
                .GetAsync(
                    m => m.Id == messageId && m.ReceiverId == userId && !m.IsDeleted, tracked: true
                );

            if (message == null)
            {
                // Message not found or doesn't belong to the user
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("Message not found or access denied");
                return _response;
            }

            // Update the message's IsFavorite property
            message.IsFavorite = isFavorite;

            // Save changes to the database
            await _unitOfWork.CompleteAsync();

            // Set the success status
            _response.StatusCode = HttpStatusCode.NoContent;

            // Return the response object
            return _response;
        }

        #endregion

        #region Get Favorited Messages
        public async Task<ApiResponse> GetFavoritedMessagesAsync(string userId)
        {
            // Fetch all non-deleted messages for the user, including sender and receiver details
            var messages = await _unitOfWork.Repositry<Message>()
                .GetAllAsync(
                    m => m.ReceiverId == userId && !m.IsDeleted && m.IsFavorite,
                    false,
                    includeProperties: "Sender,Receiver,UserReactions.Reaction"
                );


            // List to hold the mapped MessageDto objects
            var messageDtos = messages.Select(message =>
            {
                // If the message is marked as "secret", hide the sender information
                if (message.IsSecretly)
                {
                    message.Sender = null;
                }

                // Calculate reaction counts grouped by reaction type
                var reactionCounts = message.UserReactions
                    .GroupBy(ur => ur.Reaction.ReactionType)
                    .Select(group => new ReactionCountDto
                    {
                        ReactionType = group.Key,
                        Count = group.Count()
                    })
                    .ToList();

                // Map the message to its DTO and include reaction counts
                var messageDto = _mapper.Map<MessageDto>(message);
                messageDto.ReactionCounts = reactionCounts;

                return messageDto;
            }).ToList();

            // Set the success status and map the messages to DTOs
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = messageDtos;

            // Return the response object
            return _response;
        }

        #endregion

        #region Get Appeared Messages
        public async Task<ApiResponse> GetAppearedMessagesAsync(string userId)
        {
            // Fetch all non-deleted messages for the user, including sender and receiver details
            var messages = await _unitOfWork.Repositry<Message>()
                .GetAllAsync(
                    m => m.ReceiverId == userId && !m.IsDeleted && m.IsAppeared,
                    false,
                    includeProperties: "Sender,Receiver,UserReactions.Reaction"
                );

            // List to hold the mapped MessageDto objects
            var messageDtos = messages.Select(message =>
            {
                // If the message is marked as "secret", hide the sender information
                if (message.IsSecretly)
                {
                    message.Sender = null;
                }

                // Calculate reaction counts grouped by reaction type
                var reactionCounts = message.UserReactions
                    .GroupBy(ur => ur.Reaction.ReactionType)
                    .Select(group => new ReactionCountDto
                    {
                        ReactionType = group.Key,
                        Count = group.Count()
                    })
                    .ToList();

                // Map the message to its DTO and include reaction counts
                var messageDto = _mapper.Map<MessageDto>(message);
                messageDto.ReactionCounts = reactionCounts;

                return messageDto;
            }).ToList();

            // Set the success status and map the messages to DTOs
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = messageDtos;

            // Return the response object
            return _response;
        }

        #endregion


    }
}
