using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SarhneApp.Core.DTOs;
using SarhneApp.Core.Identity;
using SarhneApp.Core.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace SarhneApp.Service.Test
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<UserManager<User>> _userManagerMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<IFileService> _fileServiceMock;
        private AuthService _authService;
        private RegisterDto _registerDto;

        [SetUp]
        public void Setup()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _fileServiceMock = new Mock<IFileService>();
            _authService = new(_userManagerMock.Object, _httpContextAccessorMock.Object,_fileServiceMock.Object);
            _registerDto = new RegisterDto
            {
                Email = "test@gmail.com",
                UserName = "testUser",
                Password = "P@$$w0rd",
                Gender = Gender.Male,
                Name = "test",
                Image = new FormFile(new MemoryStream(), 0, 0, "name", "fileName")
            };
        }

        #region Register
        [Test]
        public async Task RegisterAsync_UserAlreadyExistsByEmail_ReturnsBadRequest()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByEmailAsync(_registerDto.Email))
                .ReturnsAsync(new User());
            //Act
            var response = await _authService.RegisterAsync(_registerDto);
            // Assert
            ClassicAssert.IsFalse(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            ClassicAssert.Contains("User already exists with this email", response.ErrorMessages);
        }

        [Test]
        public async Task RegisterAsync_UserAlreadyExistsByUsername_ReturnsBadRequest()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByNameAsync(_registerDto.UserName))
                .ReturnsAsync(new User());
            //Act
            var response = await _authService.RegisterAsync(_registerDto);
            //Assert
            ClassicAssert.IsFalse(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            ClassicAssert.Contains("Username is already taken", response.ErrorMessages);
        }

        [Test]
        public async Task RegisterAsync_ValidRequest_CreatesUserSuccessfully()
        {
            // Arrange
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), _registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _fileServiceMock.Setup(x => x.SaveImageAsync(_registerDto.Image, It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            //Act
            var response = await _authService.RegisterAsync(_registerDto);
            //Assert
            ClassicAssert.IsTrue(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            ClassicAssert.IsInstanceOf<UserResponseDto>(response.Result);
        } 
        #endregion


    }
}
