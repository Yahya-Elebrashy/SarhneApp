using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
    internal class AuthServiceTests
    {
        private Mock<UserManager<User>> _userManagerMock;
        private Mock<SignInManager<User>> _signInManagerMock;
        private Mock<IFileService> _fileServiceMock;
        private Mock<ITokenService> _tokenServiceMock;
        private AuthService _authService;
        private RegisterDto _registerDto;
        private LoginDto _loginDto;
        private string _testDirectory;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private new Mock<IUserClaimsPrincipalFactory<User>> _userPrincipalFactory;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IGenericRepository<User>> _genericRepositoryMock;


        [SetUp]
        public void Setup()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _signInManagerMock = new Mock<SignInManager<User>>(_userManagerMock.Object,_httpContextAccessorMock.Object,_userPrincipalFactory.Object, null, null, null, null);
            _fileServiceMock = new Mock<IFileService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _genericRepositoryMock = new Mock<IGenericRepository<User>>();
            _authService = new AuthService(_userManagerMock.Object, _fileServiceMock.Object, _tokenServiceMock.Object, _signInManagerMock.Object, _unitOfWorkMock.Object);

            _registerDto = new RegisterDto
            {
                Email = "test@gmail.com",
                UserName = "testUser",
                Password = "P@$$w0rd",
                Gender = Gender.Male,
                Name = "test",
                Image = new FormFile(new MemoryStream(), 0, 0, "name", "fileName")
            };
            _loginDto = new LoginDto
            {
                Email = "test.com",
                Password = "Test@2"
            };

            // Setup test directory
            _testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserImage");
            if (!Directory.Exists(_testDirectory))
            {
                Directory.CreateDirectory(_testDirectory);
            }
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
            string expectedFileName = "generated-filename.jpg";
            _fileServiceMock.Setup(x => x.GenerateFileName(It.IsAny<string>()))
                .Returns(expectedFileName);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), _registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _fileServiceMock.Setup(x => x.SaveImageAsync(_registerDto.Image, It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _genericRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>(),false,null))
            .ReturnsAsync((User)null);
            _unitOfWorkMock.Setup(uow => uow.Repositry<User>()).Returns(_genericRepositoryMock.Object);
            
            //Act
            var response = await _authService.RegisterAsync(_registerDto);
            //Assert
            ClassicAssert.IsTrue(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            ClassicAssert.IsInstanceOf<UserResponseDto>(response.Result);
        }
        #endregion

        #region Login
        [Test]
        public async Task LoginAsync_UserDoesNotExist_ReturnsBadRequest()
        {
            _userManagerMock.Setup(u => u.FindByEmailAsync(_loginDto.Email))
                .ReturnsAsync((User)null);
            // Act
            var response = await _authService.LoginAsync(_loginDto);
            // Assert
            ClassicAssert.IsFalse(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            ClassicAssert.Contains("User is Not Exist", response.ErrorMessages);
        }

        [Test]
        public async Task LoginAsync_InvalidPassword_ReturnsUnauthorized()
        {
            _userManagerMock.Setup(u => u.FindByEmailAsync(_loginDto.Email))
                .ReturnsAsync(new User());
            _signInManagerMock.Setup(u => u.CheckPasswordSignInAsync(It.IsAny<User>(), _loginDto.Password, false))
                .ReturnsAsync(SignInResult.Failed);
            // Act
            var response = await _authService.LoginAsync(_loginDto);
            // Assert
            ClassicAssert.IsFalse(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            ClassicAssert.Contains("Unauthorized access. Invalid credentials.", response.ErrorMessages);

        }
        [Test]
        public async Task LoginAsync_ValidLogin_ReturnsOk()
        {
            var refreshToken = new RefreshToken { Token = "refreshToken", ExpiresOn = DateTime.UtcNow.AddDays(10) };
            var user = new User { Id = "1", Email = "test@example.com", UserName = "Test User" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(_loginDto.Email))
                .ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<User>(), _loginDto.Password, false))
                .ReturnsAsync(SignInResult.Success);
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);
            _tokenServiceMock.Setup(x => x.CreateToken(user, _userManagerMock.Object))
                .ReturnsAsync("accessToken");

            // Act
            var response = await _authService.LoginAsync(_loginDto);
            var result = response.Result as UserLoginDto;            // Assert
            ClassicAssert.IsTrue(response.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            ClassicAssert.IsNotNull(response.Result);
            ClassicAssert.AreEqual("1", result.Id);
            ClassicAssert.AreEqual("test@example.com", result.Email);
            ClassicAssert.AreEqual("Test User", result.DisplayName);
            ClassicAssert.AreEqual("accessToken", result.Token);
            ClassicAssert.AreEqual("refreshToken", result.RefreshToken);
        }
        #endregion

        #region  Revoke Token
        [Test]
        public async Task RevokeTokenAsync_UserNotFound_ShouldReturnFalse()
        {
            // Arrange
            var refreshToken = "invalid-refresh-token";
            _genericRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), false, null))
            .ReturnsAsync((User)null);
            _unitOfWorkMock.Setup(uow => uow.Repositry<User>()).Returns(_genericRepositoryMock.Object);

            // Act
            var result = await _authService.RevokeTokenAsync(refreshToken);

            // Assert
            ClassicAssert.IsFalse(result);
        }
        [Test]
        public async Task RevokeTokenAsync_InvalidToken_ShouldReturnFalse()
        {
            // Arrange
            var refreshToken = "invalid-refresh-token";
            var user = new User
            {
                RefreshTokens = new List<RefreshToken>()
            };
            _tokenServiceMock.Setup(ts => ts.ValidateRefreshToken(user, refreshToken)).Returns(false);
            _genericRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), false, null))
            .ReturnsAsync(user);
            _unitOfWorkMock.Setup(uow => uow.Repositry<User>()).Returns(_genericRepositoryMock.Object);
            // Act
            var result = await _authService.RevokeTokenAsync(refreshToken);

            // Assert
            ClassicAssert.IsFalse(result);
        }

        [Test]
        public async Task RevokeTokenAsync_ValidToken_RevokesTokenAndReturnsTrue()
        {
            // Arrange
            var refreshToken = new RefreshToken { Token = "valid_token" };
            var user = new User { RefreshTokens = new List<RefreshToken> { refreshToken } };

            _genericRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), false, null))
             .ReturnsAsync(user);
            _unitOfWorkMock.Setup(uow => uow.Repositry<User>()).Returns(_genericRepositoryMock.Object);

            _tokenServiceMock.Setup(ts => ts.ValidateRefreshToken(user, "valid_token")).Returns(true);

            // Act
            var result = await _authService.RevokeTokenAsync(refreshToken.Token);

            // Assert
            ClassicAssert.IsTrue(result);
            ClassicAssert.IsNotNull(refreshToken.RevokedOn);
            _userManagerMock.Verify(um => um.UpdateAsync(user), Times.Once);
        }

        #endregion

        #region MyRegion
        [Test]
        public async Task RefreshTokenAsync_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var refreshToken = "invalid-refresh-token";
            _genericRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), false, null))
            .ReturnsAsync((User)null);
            _unitOfWorkMock.Setup(uow => uow.Repositry<User>()).Returns(_genericRepositoryMock.Object);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
            ClassicAssert.Contains("Invalid refresh token.", result.ErrorMessages);
        }
        [Test]
        public async Task RefreshTokenAsync_InvalidRefreshToken_ReturnsUnauthorized()
        {
            // Arrange
            var refreshToken = "invalid-refresh-token";
            var user = new User
            {
                RefreshTokens = new List<RefreshToken>()
            };
            _tokenServiceMock.Setup(ts => ts.ValidateRefreshToken(user, refreshToken)).Returns(false);
            _genericRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), false, null))
            .ReturnsAsync(user);
            _unitOfWorkMock.Setup(uow => uow.Repositry<User>()).Returns(_genericRepositoryMock.Object);
            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken);

            // Assert
            ClassicAssert.IsFalse(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
            ClassicAssert.Contains("Invalid refresh token.", result.ErrorMessages);
        }
        [Test]
        public async Task RefreshTokenAsync_ValidRefreshToken_ReturnsNewTokens()
        {
            // Arrange
            var user = new User
            {
                RefreshTokens = new List<RefreshToken>
            {
                new RefreshToken { Token = "validToken", ExpiresOn = DateTime.UtcNow.AddDays(1) }
            }
            };
            var newRefreshToken = new RefreshToken { Token = "newToken", ExpiresOn = DateTime.UtcNow.AddDays(7) };
            var newJwtToken = "newJwtToken";

            _genericRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), false, null))
             .ReturnsAsync(user);
            _unitOfWorkMock.Setup(uow => uow.Repositry<User>()).Returns(_genericRepositoryMock.Object);
            _tokenServiceMock.Setup(ts => ts.ValidateRefreshToken(user, "validToken")).Returns(true);
            _tokenServiceMock.Setup(ts => ts.GenerateRefreshToken()).Returns(newRefreshToken);
            _tokenServiceMock.Setup(ts => ts.CreateToken(user, _userManagerMock.Object)).ReturnsAsync(newJwtToken);

            // Act
            var result = await _authService.RefreshTokenAsync("validToken");

            // Assert
            ClassicAssert.IsTrue(result.IsSuccess);
            ClassicAssert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var resultDto = result.Result as UserLoginDto;
            ClassicAssert.AreEqual(user.Id, resultDto.Id);
            ClassicAssert.AreEqual(newJwtToken, resultDto.Token);
            ClassicAssert.AreEqual("newToken", resultDto.RefreshToken);
            ClassicAssert.AreEqual(newRefreshToken.ExpiresOn, resultDto.RefreshTokenExpiration);
        }
            #endregion
    }
}
