using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SarhneApp.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Service.Test
{
    [TestFixture]
    internal class TokenServiceTests
    {
        private TokenService _tokenService;
        private Mock<IConfiguration> _configurationMock;

        [SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();

            var configurationData = new Dictionary<string, string>
            {
                { "JWT:Key", "supersecretkeysupersecretkeysupersecretkey" },
                { "JWT:ValidIssuer", "issuer" },
                { "JWT:ValidAudience", "audience" },
                { "JWT:DurationInDays", "1" }
            };

            _configurationMock.Setup(c => c[It.IsAny<string>()])
                .Returns<string>(key => configurationData.ContainsKey(key) ? configurationData[key] : null);

            _tokenService = new TokenService(_configurationMock.Object);
        }
        [Test]
        public async Task CreateToken_ReturnsValidToken()
        {
            // Arrange
            var user = new User { Id = "1", Name = "Test User", Email = "test@example.com" };
            var userManagerMock = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new[] { "Admin" });
            // Act
            var token = await _tokenService.CreateToken(user, userManagerMock.Object);
            // Assert
            ClassicAssert.IsNotNull(token);
        }

        [Test]
        public void GenerateRefreshToken_ReturnsValidToken()
        {
            // Act
            var token = _tokenService.GenerateRefreshToken();

            // Assert
            ClassicAssert.IsNotNull(token);
            ClassicAssert.IsFalse(string.IsNullOrEmpty(token.Token));
            ClassicAssert.AreEqual(9, (token.ExpiresOn - DateTime.UtcNow).Days);
        }
    }
}
