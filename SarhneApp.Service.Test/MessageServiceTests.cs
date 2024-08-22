using AutoMapper;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SarhneApp.Api.Helper;
using SarhneApp.Core;
using SarhneApp.Core.DTOs;
using SarhneApp.Core.Entities;
using SarhneApp.Core.Identity;
using SarhneApp.Core.Repositories.Contract;
using SarhneApp.Core.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Service.Test
{
    [TestFixture]
    internal class MessageServiceTests
    {
        private MessageService _messageService;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IFileService> _fileServiceMock;
        private Mock<IGenericRepository<Message>> _messageGenericRepositoryMock;
        private IMapper _mapper;
        private Mock<IMapper> _mapperMock;

        [SetUp]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _fileServiceMock = new Mock<IFileService>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingConfig>();

            });

            _mapper = config.CreateMapper();
            _mapperMock = new Mock<IMapper>();
            _messageGenericRepositoryMock = new Mock<IGenericRepository<Message>>();
            _messageService = new MessageService(_unitOfWorkMock.Object, _fileServiceMock.Object, _mapper);
        }
        [Test]
        public void Should_Map_Message_To_MessageDto_Correctly()
        {
            // Arrange
            var message = new Message
            {
                Id = 1,
                SenderId = "aa-bb",
                Sender = new User { Id = "aa-bb" },
                ReceiverId = "cc-dd",
                Receiver = new User { Id = "cc-dd" },
                MessageText = "Hello test",
                ImageUrl = "http://example.com/image.png",
                IsFavorite = false,
                IsSecretly = false,
                IsAppeared = true,
                IsDeleted = false,
                DateOfCreation = new DateTime(2024, 8, 12, 14, 30, 0)
            };

            // Act
            var messageDto = _mapper.Map<MessageDto>(message);

            // Assert
            ClassicAssert.AreEqual(message.Id, messageDto.Id);
            ClassicAssert.AreEqual("aa-bb", messageDto.Sender.Id);
            ClassicAssert.AreEqual("cc-dd", messageDto.Receiver.Id);
            ClassicAssert.AreEqual(message.MessageText, messageDto.MessageText);
            ClassicAssert.AreEqual(message.ImageUrl, messageDto.ImageUrl);
            ClassicAssert.AreEqual(message.IsFavorite, messageDto.IsFavorite);
            ClassicAssert.AreEqual(message.IsSecretly, messageDto.IsSecretly);
            ClassicAssert.AreEqual(message.IsAppeared, messageDto.IsAppeared);
            ClassicAssert.AreEqual(message.DateOfCreation.ToString("yyyy-MM-dd HH:mm:ss"), messageDto.DateOfCreation);
        }

        #region  Send Message
        [Test]
        public async Task SendMessageAsync_MessageTextAndImageAreEmpty_ShouldReturnBadRequest()
        {
            // Arrange
            var messageDto = new SendMessageDto { MessageText = string.Empty, Image = null };

            // Act
            var result = await _messageService.SendMessageAsync(messageDto, "senderId", true);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            ClassicAssert.Contains("Image and message text cannot be empty.", result.ErrorMessages);
        }

        [Test]
        public async Task SendMessageAsync_UserIsNotAuthenticatedAndNotSecret_ShouldReturnUnauthorized()
        {
            // Arrange
            var messageDto = new SendMessageDto { MessageText = "Test", IsSecretly = false };

            // Act
            var result = await _messageService.SendMessageAsync(messageDto, null, false);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
            ClassicAssert.Contains("User is not authenticated.", result.ErrorMessages);
        }

        [Test]
        public async Task SendMessageAsync_MessageSavedSuccessfully_ShouldReturnCreatedStatus()
        {
            // Arrange
            var messageDto = new SendMessageDto { MessageText = "Hello", Image = null };
            var message = new Message();
            _mapperMock.Setup(m => m.Map<Message>(messageDto)).Returns(message);
            _unitOfWorkMock.Setup(u => u.Repositry<Message>().AddAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _unitOfWorkMock.Setup(u => u.Repositry<Message>().GetAsync(It.IsAny<Expression<Func<Message, bool>>>(), false, "Sender,Receiver"))
                .ReturnsAsync(message);

            // Act
            var result = await _messageService.SendMessageAsync(messageDto, "senderId", true);

            // Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        [Test]
        public async Task SendMessageAsync_ErrorSavingMessage_ShouldReturnInternalServerError()
        {
            // Arrange
            var messageDto = new SendMessageDto { MessageText = "Hello", Image = null };
            var message = new Message { };
            _mapperMock.Setup(m => m.Map<Message>(messageDto)).Returns(message);
            _unitOfWorkMock.Setup(u => u.Repositry<Message>().AddAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _messageService.SendMessageAsync(messageDto, "senderId", true);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            ClassicAssert.Contains("Error sending message.", result.ErrorMessages);
        }
        #endregion

        #region GetReceivedMessages
        [Test]
        public async Task GetReceivedMessagesAsync_ShouldReturnMessagesForUser_ForSecretMessages()
        {
            // Arrange
            var message = new List<Message>
            {
                new Message{ ReceiverId = "TestId", MessageText = "Hello" , IsSecretly =  true , Sender = new User{ Id = "SenderId"} },
                new Message{ ReceiverId = "TestId", MessageText = "Hello" , IsSecretly =  false , Sender = new User{ Id = "SenderId"} }
            };

            _unitOfWorkMock.Setup(u => u.Repositry<Message>().GetAllAsync(It.IsAny<Expression<Func<Message, bool>>>(), false, "Sender,Receiver,AppearedReplies,UserReactions.Reaction"))
                           .ReturnsAsync(message);
            var messageDto = new List<MessageDto>
            {
                new MessageDto{MessageText = "Hello" , IsSecretly =  true , Sender = null},
                new MessageDto{MessageText = "Hello" , IsSecretly =  false , Sender = new UserDto{Id = "SenderId"} }
            };

            //Act 
            var result = await _messageService.GetReceivedMessagesAsync("UserId");

            //Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            ClassicAssert.IsInstanceOf<IReadOnlyList<MessageDto>>(result.Result);
            ClassicAssert.AreEqual(2, ((IReadOnlyList<MessageDto>)result.Result).Count);
            ClassicAssert.IsNull(((IReadOnlyList<MessageDto>)result.Result).First().Sender);
        }

        [Test]
        public async Task GetReceivedMessagesAsync_ReturnsSuccessResponse_WithMappedMessages()
        {
            // Arrange
            var userId = "user123";
            var messages = new List<Message>
            {
                new Message
                {
                    Id = 1,
                    ReceiverId = userId,
                    IsDeleted = false,
                    IsSecretly = false,
                    SenderId = "sender1",
                    Sender = new User { Id = "sender1", UserName = "Sender One" },
                    Receiver = new User { Id = userId, UserName = "Receiver One" },
                    MessageText = "Hello from Sender One!",
                    ImageUrl = "http://example.com/image1.png",
                    IsFavorite = true,
                    IsAppeared = true,
                    DateOfCreation = new DateTime(2024, 8, 12, 14, 30, 0)
                },
                new Message
                {
                    Id = 2,
                    ReceiverId = userId,
                    IsDeleted = false,
                    IsSecretly = true,
                    SenderId = "sender2",
                    Sender = new User { Id = "sender2", UserName = "Sender Two" },
                    Receiver = new User { Id = userId, UserName = "Receiver One" },
                    MessageText = "This is a secret message from Sender Two.",
                    ImageUrl = "http://example.com/image2.png",
                    IsFavorite = false,
                    IsAppeared = false,
                    DateOfCreation = new DateTime(2024, 8, 13, 10, 0, 0)
                }
            };
            _unitOfWorkMock.Setup(uow => uow.Repositry<Message>().GetAllAsync(It.IsAny<Expression<Func<Message, bool>>>(), false, It.IsAny<string>()))
                           .ReturnsAsync(messages);
            var expectedDtos = new List<MessageDto>
            {
                new MessageDto
                {
                    Id = 1,
                    Sender = new UserDto { Id = "sender1"},
                    Receiver = new UserDto { Id = userId },
                    MessageText = "Hello from Sender One!",
                    ImageUrl = "http://example.com/image1.png",
                    IsFavorite = true,
                    IsSecretly = false,
                    IsAppeared = true,
                    DateOfCreation = "2024-08-12 14:30:00"
                },
                new MessageDto
                {
                    Id = 2,
                    Sender = null,
                    Receiver = new UserDto { Id = userId},
                    MessageText = "This is a secret message from Sender Two.",
                    ImageUrl = "http://example.com/image2.png",
                    IsFavorite = false,
                    IsSecretly = true,
                    IsAppeared = false,
                    DateOfCreation = "2024-08-13 10:00:00"
                }
            };
            // Act
            var response = await _messageService.GetReceivedMessagesAsync(userId);

            // Assert
            ClassicAssert.IsTrue(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            ClassicAssert.IsNotNull(response.Result);
            var resultDtos = response.Result as List<MessageDto>;
            ClassicAssert.AreEqual(expectedDtos.Count, resultDtos.Count);

            for (int i = 0; i < expectedDtos.Count; i++)
            {
                var expectedDto = expectedDtos[i];
                var resultDto = resultDtos[i];

                ClassicAssert.AreEqual(expectedDto.Id, resultDto.Id);
                ClassicAssert.AreEqual(expectedDto.Sender?.Id, resultDto.Sender?.Id);
                ClassicAssert.AreEqual(expectedDto.Receiver.Id, resultDto.Receiver.Id);
                ClassicAssert.AreEqual(expectedDto.MessageText, resultDto.MessageText);
                ClassicAssert.AreEqual(expectedDto.ImageUrl, resultDto.ImageUrl);
                ClassicAssert.AreEqual(expectedDto.IsFavorite, resultDto.IsFavorite);
                ClassicAssert.AreEqual(expectedDto.IsSecretly, resultDto.IsSecretly);
                ClassicAssert.AreEqual(expectedDto.IsAppeared, resultDto.IsAppeared);
                ClassicAssert.AreEqual(expectedDto.DateOfCreation, resultDto.DateOfCreation);
            }

        }
        #endregion

        #region Get Sent Messages
        [Test]
        public async Task GetSentMessagesAsync_SuccessfullyGetSentMessages_ShouldReturnOkStatus()
        {
            // Arrange
            var senderId = "SenderId";
            var message = new List<Message>
            {
                new Message{ ReceiverId = "TestId", MessageText = "Hello" , IsSecretly =  true , Sender = null },
                new Message{ ReceiverId = "TestId", MessageText = "Hello" , IsSecretly =  false , Sender = new User{ Id = senderId} }
            };

            _unitOfWorkMock.Setup(u => u.Repositry<Message>().GetAllAsync(It.IsAny<Expression<Func<Message, bool>>>(), false, "Sender,Receiver,AppearedReplies,UserReactions.Reaction"))
                           .ReturnsAsync(message.Where(m => m.Sender != null && m.Sender.Id == senderId).ToList());
            var messageDto = new List<MessageDto>
            {
                new MessageDto{MessageText = "Hello" , IsSecretly =  false , Sender = new UserDto{Id = "SenderId"} }
            };

            //Act
            var result = await _messageService.GetSentMessagesAsync(senderId);

            //Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            ClassicAssert.IsInstanceOf<IReadOnlyList<MessageDto>>(result.Result);
            ClassicAssert.AreEqual(1, ((IReadOnlyList<MessageDto>)result.Result).Count);
            ClassicAssert.IsNotNull(((IReadOnlyList<MessageDto>)result.Result).First().Sender);
        }
        #endregion

        #region Delete Received Message
        [Test]
        public async Task DeleteReceivedMessageAsync_MessageNotFound_ReturnsNotFound()
        {
            // Arrange
            string userId = "userId";
            int messageId = 1;
            _unitOfWorkMock.Setup(uow => uow.Repositry<Message>().GetAsync(
             It.IsAny<Expression<Func<Message, bool>>>(),
             true, 
             "AppearedReplies" 
             )).ReturnsAsync((Message)null);

            // Act
            var result = await _messageService.DeleteReceivedMessageAsync(messageId, userId);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            ClassicAssert.Contains("Message not found or access denied", result.ErrorMessages);
        }

        [Test]
        public async Task DeleteReceivedMessageAsync_SuccessfulDeletion_ReturnsNoContent()
        {
            // Arrange
            string userId = "userId";
            var message = new Message { Id = 1, MessageText = "Hello", ReceiverId = "ReceiverId" };
            _unitOfWorkMock.Setup(uow => uow.Repositry<Message>().GetAsync(
             It.IsAny<Expression<Func<Message, bool>>>(),
             true,
             "AppearedReplies"
             )).ReturnsAsync(message);

            _unitOfWorkMock.Setup(uof => uof.Repositry<ReplyAppearedMessage>().DeleteRangeAsync(message.AppearedReplies))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.CompleteAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _messageService.DeleteReceivedMessageAsync(message.Id, userId);

            // Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task DeleteReceivedMessageAsync_DeletionFails_ReturnsInternalServerError()
        {
            // Arrange
            string userId = "userId";
            var message = new Message { Id = 1, MessageText = "Hello", ReceiverId = "ReceiverId" };
            _unitOfWorkMock.Setup(uow => uow.Repositry<Message>().GetAsync(
             It.IsAny<Expression<Func<Message, bool>>>(),
             true,
             "AppearedReplies"
             )).ReturnsAsync(message);

            _unitOfWorkMock.Setup(uof => uof.Repositry<ReplyAppearedMessage>().DeleteRangeAsync(message.AppearedReplies))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.CompleteAsync())
                .ReturnsAsync(0);

            // Act
            var result = await _messageService.DeleteReceivedMessageAsync(message.Id, userId);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            ClassicAssert.Contains("Error deleting message.", result.ErrorMessages);
        }
        #endregion

        #region UpdateAppearedMessage
        [Test]
        public async Task UpdateAppearedMessageAsync_MessageNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var messageId = 1;
            var userId = "userId";
            var isAppeared = true;

            _unitOfWorkMock.Setup(uow => uow.Repositry<Message>()
                           .GetAsync(It.IsAny<Expression<Func<Message, bool>>>(),true, null))
                           .ReturnsAsync((Message)null);

            // Act
            var result = await _messageService.UpdateAppearedMessageAsync(messageId, userId, isAppeared);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            ClassicAssert.Contains("Message not found or access denied", result.ErrorMessages);
            _unitOfWorkMock.Verify(
                           uow => uow.Repositry<Message>().GetAsync(It.IsAny<Expression<Func<Message, bool>>>(), true, null),
                           Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
        }
        [Test]
        public async Task UpdateAppearedMessageAsync_MessageExists_ShouldUpdateIsAppeared()
        {
            // Arrange
            var messageId = 1;
            var userId = "userId";
            var isAppeared = true;
            var message = new Message { Id = messageId, MessageText= "Test" , ReceiverId = userId };
            _unitOfWorkMock.Setup(uow => uow.Repositry<Message>()
                           .GetAsync(It.IsAny<Expression<Func<Message, bool>>>(), true, null))
                           .ReturnsAsync(message);

            // Act
            var result = await _messageService.UpdateAppearedMessageAsync(messageId, userId, isAppeared);

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
            _unitOfWorkMock.Verify(
                           uow => uow.Repositry<Message>().GetAsync(It.IsAny<Expression<Func<Message, bool>>>(), true, null),
                           Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }
        #endregion

        #region UpdateFavoriteMessage
        [Test]
        public async Task UpdateFavoriteMessageAsync_MessageNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var messageId = 1;
            var userId = "userId";
            var isAppeared = true;

            _unitOfWorkMock.Setup(uow => uow.Repositry<Message>()
                           .GetAsync(It.IsAny<Expression<Func<Message, bool>>>(), true, null))
                           .ReturnsAsync((Message)null);

            // Act
            var result = await _messageService.UpdateFavoriteAsync(messageId, userId, isAppeared);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            ClassicAssert.Contains("Message not found or access denied", result.ErrorMessages);
            _unitOfWorkMock.Verify(
                           uow => uow.Repositry<Message>().GetAsync(It.IsAny<Expression<Func<Message, bool>>>(), true, null),
                           Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
        }
        [Test]
        public async Task UpdateFavoriteMessageAsync_MessageExists_ShouldUpdateIsAppeared()
        {
            // Arrange
            var messageId = 1;
            var userId = "userId";
            var isAppeared = true;
            var message = new Message { Id = messageId, MessageText = "Test", ReceiverId = userId };
            _unitOfWorkMock.Setup(uow => uow.Repositry<Message>()
                           .GetAsync(It.IsAny<Expression<Func<Message, bool>>>(), true, null))
                           .ReturnsAsync(message);

            // Act
            var result = await _messageService.UpdateFavoriteAsync(messageId, userId, isAppeared);

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
            _unitOfWorkMock.Verify(
                           uow => uow.Repositry<Message>().GetAsync(It.IsAny<Expression<Func<Message, bool>>>(), true, null),
                           Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }
        #endregion

        #region Get Favorited Messages
        [Test]
        public async Task GetFavoritedMessagesAsync_ReturnsMessagesWithSenderHidden_WhenMessageIsSecret()
        {
            // Arrange
            var userId = "testUserId";
            var messages = new List<Message>
            {
                new Message { Id = 1, ReceiverId = userId, IsDeleted = false, IsFavorite = true, IsSecretly = true, Sender = new User { Id = "sender1" }, Receiver = new User { Id = userId } },
                new Message { Id = 2, ReceiverId = userId, IsDeleted = false, IsFavorite = true, IsSecretly = false, Sender = new User { Id = "sender2" }, Receiver = new User { Id = userId } }
            };

            _unitOfWorkMock
                    .Setup(u => u.Repositry<Message>().GetAllAsync(It.IsAny<Expression<Func<Message, bool>>>(), false, "Sender,Receiver,UserReactions.Reaction"))
                    .ReturnsAsync(messages);

            var expectedDtos = new List<MessageDto>
            {
                new MessageDto { Id = 1, Sender = null, Receiver = new UserDto { Id = userId } },  // Sender should be null
                new MessageDto { Id = 2, Sender = new UserDto { Id = "sender2" }, Receiver = new UserDto { Id = userId } }
            };

            _mapperMock.Setup(m => m.Map<IReadOnlyList<MessageDto>>(messages)).Returns(expectedDtos);

            //Act
            var result = await _messageService.GetFavoritedMessagesAsync(userId);

            //Asseret
            ClassicAssert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.IsInstanceOf<IReadOnlyList<MessageDto>>(result.Result);
            var resultList = result.Result as IReadOnlyList<MessageDto>;
            ClassicAssert.AreEqual(2, resultList.Count);
            ClassicAssert.IsNull(resultList.First().Sender);
        }
        #endregion-

        #region Get Appeared Messages
        [Test]
        public async Task GetAppearedMessagesAsync_ReturnsMessagesWithSenderHidden_WhenMessageIsSecret()
        {
            // Arrange
            var userId = "testUserId";
            var messages = new List<Message>
            {
                new Message { Id = 1, ReceiverId = userId, IsDeleted = false, IsFavorite = true, IsSecretly = true, Sender = new User { Id = "sender1" }, Receiver = new User { Id = userId } },
                new Message { Id = 2, ReceiverId = userId, IsDeleted = false, IsFavorite = true, IsSecretly = false, Sender = new User { Id = "sender2" }, Receiver = new User { Id = userId } }
            };

            _unitOfWorkMock
                    .Setup(u => u.Repositry<Message>().GetAllAsync(It.IsAny<Expression<Func<Message, bool>>>(), false, "Sender,Receiver,UserReactions.Reaction"))
                    .ReturnsAsync(messages);

            var expectedDtos = new List<MessageDto>
            {
                new MessageDto { Id = 1, Sender = null, Receiver = new UserDto { Id = userId } },  // Sender should be null
                new MessageDto { Id = 2, Sender = new UserDto { Id = "sender2" }, Receiver = new UserDto { Id = userId } }
            };

            _mapperMock.Setup(m => m.Map<IReadOnlyList<MessageDto>>(messages)).Returns(expectedDtos);

            //Act
            var result = await _messageService.GetAppearedMessagesAsync(userId);

            //Asseret
            ClassicAssert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.IsInstanceOf<IReadOnlyList<MessageDto>>(result.Result);
            var resultList = result.Result as IReadOnlyList<MessageDto>;
            ClassicAssert.AreEqual(2, resultList.Count);
            ClassicAssert.IsNull(resultList.First().Sender);
        }
        #endregion


    }
}
