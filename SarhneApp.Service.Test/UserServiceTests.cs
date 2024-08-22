using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SarhneApp.Core;
using SarhneApp.Core.DTOs;
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
    internal class UserServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private Mock<UserManager<User>> _userManagerMock;
        private UserService _userService;
        [SetUp]
        public void SetUp() {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _userService = new UserService(_unitOfWorkMock.Object, _mapperMock.Object, _userManagerMock.Object);
        }

        #region Get User By Link Async
        [Test]
        public async Task GetUserByLinkAsync_LinkIsNullOrEmpty_ReturnsBadRequest()
        {
            // Arrange
            string link = null;

            // Act
            var result = await _userService.GetUserByLinkAsync(link);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            ClassicAssert.Contains("invalid link", result.ErrorMessages);
        }

        [Test]
        public async Task GetUserByLinkAsync_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            string link = "link";

            _unitOfWorkMock.Setup(u => u.Repositry<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(),It.IsAny<bool>(),It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByLinkAsync(link);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            ClassicAssert.Contains("user NotFound", result.ErrorMessages);
        }

        [Test]
        public async Task GetUserByLinkAsync_UserFound_ReturnsOk()
        {
            // Arrange
            string link = "link";
            var user = new User { Id = "userId", Name = "test name" };
            var userDto = new UserDto { Id = "userId", Name = "test name" };

            _unitOfWorkMock.Setup(u => u.Repositry<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync(user);

            _mapperMock.Setup(m => m.Map<UserDto>(It.IsAny<User>()))
                .Returns(userDto);

            // Act
            var result = await _userService.GetUserByLinkAsync(link);

            // Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            ClassicAssert.AreEqual(userDto, result.Result);
        }
        #endregion

        #region Update User Data
        [Test]
        public async Task UpdateUserDataAsync_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = "userId";
            var updateUserDataDto = new UpdateUserDataDto { Gender = Gender.Male, Name = "John Doe", DetailsAboutMe = "Some details" };

            _unitOfWorkMock.Setup(u => u.Repositry<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(), true, It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.UpdateUserDataAsync(userId, updateUserDataDto);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            ClassicAssert.Contains("User not found.", result.ErrorMessages);
        }

        [Test]
        public async Task UpdateUserDataAsync_SuccessfulUpdate_ReturnsOk()
        {
            // Arrange
            var userId = "userId";
            var updateUserDataDto = new UpdateUserDataDto { Gender = Gender.Male, Name = "John Doe", DetailsAboutMe = "Some details" };
            var user = new User { Id = userId, Gender = Gender.Female, Name = "Jane Doe", DetailsAboutMe = "Old details" };
            var userDto = new UserDto { Gender = Gender.Male, Name = "John Doe", DetailsAboutMe = "Some details" };

            _unitOfWorkMock.Setup(u => u.Repositry<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(), true, It.IsAny<string>()))
                .ReturnsAsync(user);

            _mapperMock.Setup(m => m.Map<UserDto>(It.IsAny<User>()))
                .Returns(userDto);

            // Act
            var result = await _userService.UpdateUserDataAsync(userId, updateUserDataDto);

            // Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            ClassicAssert.AreEqual(userDto, result.Result);
        }

        [Test]
        public async Task UpdateUserDataAsync_SaveChangesFails_ThrowsException()
        {
            // Arrange
            var userId = "userId";
            var updateUserDataDto = new UpdateUserDataDto { Gender = Gender.Male, Name = "John Doe", DetailsAboutMe = "Some details" };
            var user = new User { Id = userId };

            _unitOfWorkMock.Setup(u => u.Repositry<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(), true, It.IsAny<string>()))
                .ReturnsAsync(user);

            _unitOfWorkMock.Setup(u => u.CompleteAsync())
                .ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _userService.UpdateUserDataAsync(userId, updateUserDataDto));

            ClassicAssert.AreEqual("Database save failed", ex.Message);
        }

        #endregion

        #region Update User Email
        [Test]
        public async Task UpdateEmailAsync_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = "userId";
            var updateEmailDto = new UpdateUserEmailDto { NewEmail = "newemail@example.com" };

            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.UpdateEmailAsync(userId, updateEmailDto);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            ClassicAssert.Contains("User not found.", result.ErrorMessages);
        }

        [Test]
        public async Task UpdateEmailAsync_EmailUpdateFails_ReturnsBadRequest()
        {
            // Arrange
            var userId = "userId";
            var updateEmailDto = new UpdateUserEmailDto { NewEmail = "newemail@example.com" };
            var user = new User { Id = userId };

            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(u => u.GenerateChangeEmailTokenAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync("token");

            _userManagerMock.Setup(u => u.ChangeEmailAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _userService.UpdateEmailAsync(userId, updateEmailDto);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            ClassicAssert.Contains("invalid changing email", result.ErrorMessages);
        }

        [Test]
        public async Task UpdateEmailAsync_SuccessfulEmailUpdate_ReturnsOk()
        {
            // Arrange
            var userId = "userId";
            var updateEmailDto = new UpdateUserEmailDto { NewEmail = "newemail@example.com" };
            var user = new User { Id = userId };
            var userDto = new UserDto { Id = userId, Email = "newemail@example.com" };

            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(u => u.GenerateChangeEmailTokenAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync("token");

            _userManagerMock.Setup(u => u.ChangeEmailAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mapperMock.Setup(m => m.Map<UserDto>(It.IsAny<User>()))
                .Returns(userDto);

            // Act
            var result = await _userService.UpdateEmailAsync(userId, updateEmailDto);

            // Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            ClassicAssert.AreEqual(userDto, result.Result);
        }
        #endregion

        #region Update Password
        [Test]
        public async Task UpdatePasswordAsync_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = "userId";
            var updatePasswordDto = new UpdateUserPasswordDto { CurrentPassword = "currentPassword", NewPassword = "newPassword" };

            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.UpdatePasswordAsync(userId, updatePasswordDto);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            ClassicAssert.Contains("User not found.", result.ErrorMessages);
        }

        [Test]
        public async Task UpdatePasswordAsync_PasswordUpdateFails_ReturnsBadRequest()
        {
            // Arrange
            var userId = "userId";
            var updatePasswordDto = new UpdateUserPasswordDto { CurrentPassword = "currentPassword", NewPassword = "newPassword" };
            var user = new User { Id = userId };

            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(u => u.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _userService.UpdatePasswordAsync(userId, updatePasswordDto);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            ClassicAssert.Contains("invalid changing password!", result.ErrorMessages);
        }

        [Test]
        public async Task UpdatePasswordAsync_SuccessfulPasswordUpdate_ReturnsOk()
        {
            // Arrange
            var userId = "userId";
            var updatePasswordDto = new UpdateUserPasswordDto { CurrentPassword = "currentPassword", NewPassword = "newPassword" };
            var user = new User { Id = userId };
            var userDto = new UserDto { Id = userId };

            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(u => u.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mapperMock.Setup(m => m.Map<UserDto>(It.IsAny<User>()))
                .Returns(userDto);

            // Act
            var result = await _userService.UpdatePasswordAsync(userId, updatePasswordDto);

            // Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            ClassicAssert.AreEqual(userDto, result.Result);
        }
        #endregion


    }
}
