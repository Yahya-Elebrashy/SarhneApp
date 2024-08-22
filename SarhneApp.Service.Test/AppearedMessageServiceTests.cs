using AutoMapper;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SarhneApp.Api.Helper;
using SarhneApp.Core;
using SarhneApp.Core.DTOs;
using SarhneApp.Core.Entities;
using SarhneApp.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Service.Test
{
    internal class AppearedMessageServiceTests
    {
        private IMapper _mapper;
        private Mock<IMapper> _mapperMock;
        private Mock<IUnitOfWork> _unitOfWork;
        private AppearedMessageService _appearedMessageService;
        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingConfig>();

            });

            _mapper = config.CreateMapper();
            _mapperMock = new Mock<IMapper>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _appearedMessageService = new(_unitOfWork.Object, _mapperMock.Object);
        }
        #region Add Reply Appeared Message
        [Test]
        public async Task AddReplyAppearedMessageAsync_MessageNotFound_ReturnsNotFound()
        {
            // Arrange
            var dto = new AddReplyAppearedMessageDto { MessageId = 1, ReplyText = "twst" };
            string userId = "user123";

            _unitOfWork.Setup(u => u.Repositry<Message>().GetAsync(
                It.IsAny<Expression<Func<Message, bool>>>(),
                false,
                It.IsAny<string>()
            )).ReturnsAsync((Message)null);

            // Act
            var result = await _appearedMessageService.AddReplyAppearedMessageAsync(dto, userId);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            ClassicAssert.Contains("Message not found or access denied", result.ErrorMessages);
        }
        [Test]
        public async Task AddReplyAppearedMessageAsync_MessageNotAppeared_ReturnsBadRequest()
        {
            // Arrange
            var message = new Message { Id = 1, ReceiverId = "userId", IsAppeared = false };
            // Arrange
            var dto = new AddReplyAppearedMessageDto { MessageId = 1, ReplyText = "twst" };
            string userId = "user123";

            _unitOfWork.Setup(u => u.Repositry<Message>().GetAsync(
                It.IsAny<Expression<Func<Message, bool>>>(),
                false,
                It.IsAny<string>()
            )).ReturnsAsync(message);

            // Act
            var result = await _appearedMessageService.AddReplyAppearedMessageAsync(dto, userId);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            ClassicAssert.Contains("Message must be appeared", result.ErrorMessages);
        }

        [Test]
        public async Task AddReplyAppearedMessageAsync_SuccessfulReply_ReturnsCreated()
        {
            // Arrange
            var message = new Message { Id = 1, ReceiverId = "userId", IsAppeared = true, IsSecretly = true, Sender = new User() };
            var dto = new AddReplyAppearedMessageDto { MessageId = 1, ReplyText = "test" };
            var replyAppearedMessageDto = new ReplyAppearedMessageDto { Id = 1, ReplyText = "test" };
            string userId = "user123";
            var replyAppearedMessage = new ReplyAppearedMessage();

            _unitOfWork.Setup(u => u.Repositry<Message>().GetAsync(
               It.IsAny<Expression<Func<Message, bool>>>(),
               false,
               It.IsAny<string>()
            )).ReturnsAsync(message);

            _mapperMock.Setup(m => m.Map<ReplyAppearedMessage>(dto))
                .Returns(replyAppearedMessage);

            _unitOfWork.Setup(u => u.Repositry<ReplyAppearedMessage>().AddAsync(It.IsAny<ReplyAppearedMessage>()))
                .Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<ReplyAppearedMessageDto>(replyAppearedMessage)).Returns(replyAppearedMessageDto);
            _unitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _appearedMessageService.AddReplyAppearedMessageAsync(dto, userId);

            // Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.IsNull(message.Sender);
            ClassicAssert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            ClassicAssert.IsNotNull(result.Result);
        }

        [Test]
        public async Task AddReplyAppearedMessageAsync_SaveChangesFailure_ReturnsInternalServerError()
        {
            // Arrange
            var message = new Message { Id = 1, ReceiverId = "userId", IsAppeared = true };
            var dto = new AddReplyAppearedMessageDto { MessageId = 1 };
            string userId = "user123";
            var replyAppearedMessage = new ReplyAppearedMessage();

            _unitOfWork.Setup(u => u.Repositry<Message>().GetAsync(
              It.IsAny<Expression<Func<Message, bool>>>(),
              false,
              It.IsAny<string>()
            )).ReturnsAsync(message);

            _mapperMock.Setup(m => m.Map<ReplyAppearedMessage>(dto))
                .Returns(replyAppearedMessage);

            _unitOfWork.Setup(u => u.Repositry<ReplyAppearedMessage>().AddAsync(It.IsAny<ReplyAppearedMessage>()))
                .Returns(Task.CompletedTask);

            _unitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _appearedMessageService.AddReplyAppearedMessageAsync(dto, userId);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            ClassicAssert.Contains("Error sending replay.", result.ErrorMessages);
        }

        #endregion

        #region Delete Reply Appeared Message
        [Test]
        public async Task DeleteReplyAppearedMessageAsync_MessageNotFound_ReturnsNotFound()
        {
            // Arrange
            var replyAppearedMessageId = 1;
            var userId = "user123";

            _unitOfWork.Setup(u => u.Repositry<ReplyAppearedMessage>()
                .GetAsync(It.IsAny<Expression<Func<ReplyAppearedMessage, bool>>>(),false, null))
                .ReturnsAsync((ReplyAppearedMessage)null);

            // Act
            var result = await _appearedMessageService.DeteteReplyAppearedMessageAsync(replyAppearedMessageId, userId);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            ClassicAssert.Contains("Message not found or access denied", result.ErrorMessages);
        }

        [Test]
        public async Task DeleteReplyAppearedMessageAsync_DeleteFails_ReturnsInternalServerError()
        {
            // Arrange
            var replyAppearedMessageId = 1;
            var userId = "user123";
            var replyAppearedMessage = new ReplyAppearedMessage { Id = replyAppearedMessageId };

            _unitOfWork.Setup(u => u.Repositry<ReplyAppearedMessage>()
                 .GetAsync(It.IsAny<Expression<Func<ReplyAppearedMessage, bool>>>(), false, null))
                 .ReturnsAsync(replyAppearedMessage);

            _unitOfWork.Setup(u => u.CompleteAsync())
                .ReturnsAsync(0);

            // Act
            var result = await _appearedMessageService.DeteteReplyAppearedMessageAsync(replyAppearedMessageId, userId);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            ClassicAssert.Contains("Error Deleting replay.", result.ErrorMessages);
        }

        [Test]
        public async Task DeleteReplyAppearedMessageAsync_SuccessfulDeletion_ReturnsNoContent()
        {
            // Arrange
            var replyAppearedMessageId = 1;
            var userId = "user123";
            var replyAppearedMessage = new ReplyAppearedMessage { Id = replyAppearedMessageId };

            _unitOfWork.Setup(u => u.Repositry<ReplyAppearedMessage>()
                 .GetAsync(It.IsAny<Expression<Func<ReplyAppearedMessage, bool>>>(), false, null))
                 .ReturnsAsync(replyAppearedMessage);

            _unitOfWork.Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _appearedMessageService.DeteteReplyAppearedMessageAsync(replyAppearedMessageId, userId);

            // Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
            ClassicAssert.IsEmpty(result.ErrorMessages);
        }
        #endregion

        #region Update Reply Appeared Message
        [Test]
        public async Task UpdateReplyAppearedMessageAsync_MessageNotFound_ReturnsNotFound()
        {
            // Arrange
            var replyAppearedMessageId = 1;
            var userId = "user123";
            var dto = new UpdateReplyAppearedMessageDto { ReplyText = "Updated Reply" };

            _unitOfWork.Setup(u => u.Repositry<ReplyAppearedMessage>()
                .GetAsync(It.IsAny<Expression<Func<ReplyAppearedMessage, bool>>>(),
                          It.IsAny<bool>(),
                          It.IsAny<string>()))
                .ReturnsAsync((ReplyAppearedMessage)null);

            // Act
            var result = await _appearedMessageService.UpdateReplyAppearedMessageAsync(replyAppearedMessageId, userId, dto);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            ClassicAssert.Contains("Message not found or access denied", result.ErrorMessages);
        }

        [Test]
        public async Task UpdateReplyAppearedMessageAsync_SuccessfulUpdate_ReturnsOk()
        {
            // Arrange
            var replyAppearedMessageId = 1;
            var userId = "user123";
            var dto = new UpdateReplyAppearedMessageDto { ReplyText = "Updated Reply" };
            var replyAppearedMessage = new ReplyAppearedMessage { Id = replyAppearedMessageId, ReplyText = "Old Reply" };
            var updatedMessageDto = new ReplyAppearedMessageDto { Id = replyAppearedMessageId, ReplyText = "Updated Reply" };

            _unitOfWork.Setup(u => u.Repositry<ReplyAppearedMessage>()
                .GetAsync(It.IsAny<Expression<Func<ReplyAppearedMessage, bool>>>(),
                          It.IsAny<bool>(),
                          It.IsAny<string>()))
                .ReturnsAsync(replyAppearedMessage);

            _mapperMock.Setup(m => m.Map<ReplyAppearedMessageDto>(It.IsAny<ReplyAppearedMessage>()))
                .Returns(updatedMessageDto);

            // Act
            var result = await _appearedMessageService.UpdateReplyAppearedMessageAsync(replyAppearedMessageId, userId, dto);

            // Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            ClassicAssert.AreEqual(updatedMessageDto, result.Result);
        }

        [Test]
        public async Task UpdateReplyAppearedMessageAsync_SaveChangesFails_ThrowsException()
        {
            // Arrange
            var replyAppearedMessageId = 1;
            var userId = "user123";
            var dto = new UpdateReplyAppearedMessageDto { ReplyText = "Updated Reply" };
            var replyAppearedMessage = new ReplyAppearedMessage { Id = replyAppearedMessageId, ReplyText = "Old Reply" };

            _unitOfWork.Setup(u => u.Repositry<ReplyAppearedMessage>()
                .GetAsync(It.IsAny<Expression<Func<ReplyAppearedMessage, bool>>>(),
                          It.IsAny<bool>(),
                          It.IsAny<string>()))
                .ReturnsAsync(replyAppearedMessage);

            _unitOfWork.Setup(u => u.CompleteAsync())
                .ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _appearedMessageService.UpdateReplyAppearedMessageAsync(replyAppearedMessageId, userId, dto));

            ClassicAssert.AreEqual("Database save failed", ex.Message);
        }
        #endregion
    }
}
