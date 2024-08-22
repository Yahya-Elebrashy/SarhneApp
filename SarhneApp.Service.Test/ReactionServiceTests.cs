using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SarhneApp.Api.Controllers;
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
    internal class ReactionServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private IMapper _mapper;
        private ReactionService _reactionService;
        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingConfig>();

            });

            _mapper = config.CreateMapper();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _reactionService = new ReactionService(_unitOfWorkMock.Object, _mapperMock.Object);
        }


        #region Get Reactions
        [Test]
        public async Task GetReactionsAsync_ShouldReturnSuccessResponse_WithMappedReactions()
        {
            // Arrange
            var reactions = new List<Reaction> {
                new Reaction { Id =1, ReactionType= "Test" }
            };
            var reactionDtos = new List<ReactionDto> {
                new ReactionDto {  Id =1, ReactionType= "Test" }
            };

            _unitOfWorkMock.Setup(uow => uow.Repositry<Reaction>().GetAllAsync(null, false, null))
                .ReturnsAsync(reactions);

            _mapperMock.Setup(mapper => mapper.Map<IReadOnlyList<ReactionDto>>(reactions))
                .Returns(reactionDtos);
            // Act
            var result = await _reactionService.GetReactionsAsync();

            // Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            ClassicAssert.AreEqual(reactionDtos, result.Result);
        }
        #endregion

        #region React To Message
        [Test]
        public async Task ReactToMessageAsync_InvalidUser_ReturnsBadRequest()
        {
            // Arrange
            var reactToMessageDto = new AddReactToMessageDto { MessageId = 1, ReactionId = 2 };
            _unitOfWorkMock.Setup(uow => uow.Repositry<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(),false,null))
                .ReturnsAsync((User)null);

            // Act
            var response = await _reactionService.ReactToMessageAsync(reactToMessageDto, "userId");

            // Assert
            ClassicAssert.IsFalse(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            ClassicAssert.Contains("Invalid user.", response.ErrorMessages);
        }

        [Test]
        public async Task ReactToMessageAsync_InvalidMessage_ReturnsBadRequest()
        {
            // Arrange
            var reactToMessageDto = new AddReactToMessageDto { MessageId = 1, ReactionId = 2 };
            _unitOfWorkMock.Setup(uow => uow.Repositry<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(), false, null))
                .ReturnsAsync(new User());
            _unitOfWorkMock.Setup(uow => uow.Repositry<Message>().GetAsync(It.IsAny<Expression<Func<Message, bool>>>(), false, null))
                .ReturnsAsync((Message)null);

            // Act
            var response = await _reactionService.ReactToMessageAsync(reactToMessageDto, "userId");

            // Assert
            ClassicAssert.IsFalse(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            ClassicAssert.Contains("Invalid message.", response.ErrorMessages);
        }

        [Test]
        public async Task ReactToMessageAsync_InvalidReaction_ReturnsBadRequest()
        {
            // Arrange
            var reactToMessageDto = new AddReactToMessageDto { MessageId = 1, ReactionId = 2 };
            _unitOfWorkMock.Setup(uow => uow.Repositry<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(), false, null))
                .ReturnsAsync(new User());
            _unitOfWorkMock.Setup(uow => uow.Repositry<Message>().GetAsync(It.IsAny<Expression<Func<Message, bool>>>(), false, null))
                .ReturnsAsync(new Message());
            _unitOfWorkMock.Setup(uow => uow.Repositry<Reaction>().GetAsync(It.IsAny<Expression<Func<Reaction, bool>>>(), false, null))
                 .ReturnsAsync((Reaction)null);

            // Act
            var response = await _reactionService.ReactToMessageAsync(reactToMessageDto, "userId");

            // Assert
            ClassicAssert.IsFalse(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            ClassicAssert.Contains("Invalid reaction.", response.ErrorMessages);
        }

        [Test]
        public async Task ReactToMessageAsync_ExistingReaction_UpdatesReaction()
        {
            // Arrange
            var existingReaction = new UserReaction { UserId = "userId", MessageId = 1, ReactionId = 3 };
            var reactToMessageDto = new AddReactToMessageDto { MessageId = 1, ReactionId = 2 };
            _unitOfWorkMock.Setup(uow => uow.Repositry<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(), false, null))
                .ReturnsAsync(new User());
            _unitOfWorkMock.Setup(uow => uow.Repositry<Message>().GetAsync(It.IsAny<Expression<Func<Message, bool>>>(), false, null))
                .ReturnsAsync(new Message());
            _unitOfWorkMock.Setup(uow => uow.Repositry<Reaction>().GetAsync(It.IsAny<Expression<Func<Reaction, bool>>>(), false, null))
                 .ReturnsAsync(new Reaction());
            _unitOfWorkMock.Setup(uow => uow.Repositry<UserReaction>().GetAsync(It.IsAny<Expression<Func<UserReaction, bool>>>(), true, "Message,Reaction"))
            .ReturnsAsync(existingReaction);
            // Act
            var response = await _reactionService.ReactToMessageAsync(reactToMessageDto, "userId");

            // Assert
            ClassicAssert.IsTrue(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _unitOfWorkMock.Verify(r => r.Repositry<UserReaction>().UpdateAsync(existingReaction), Times.Once);
        }

        [Test]
        public async Task ReactToMessageAsync_NewReaction_AddsReaction()
        {
            // Arrange
            var reactToMessageDto = new AddReactToMessageDto { MessageId = 1, ReactionId = 2 };
            _unitOfWorkMock.Setup(uow => uow.Repositry<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(), false, null))
                .ReturnsAsync(new User());
            _unitOfWorkMock.Setup(uow => uow.Repositry<Message>().GetAsync(It.IsAny<Expression<Func<Message, bool>>>(), false, null))
                .ReturnsAsync(new Message());
            _unitOfWorkMock.Setup(uow => uow.Repositry<Reaction>().GetAsync(It.IsAny<Expression<Func<Reaction, bool>>>(), false, null))
                 .ReturnsAsync(new Reaction());
            _unitOfWorkMock.Setup(uow => uow.Repositry<UserReaction>().GetAsync(It.IsAny<Expression<Func<UserReaction, bool>>>(), true, "Message,Reaction"))
            .ReturnsAsync((UserReaction)null);

            // Act
            var response = await _reactionService.ReactToMessageAsync(reactToMessageDto, "userId");

            // Assert
            ClassicAssert.IsTrue(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _unitOfWorkMock.Verify(r => r.Repositry<UserReaction>().AddAsync(It.IsAny<UserReaction>()), Times.Once);
        }
        #endregion
    }
}
