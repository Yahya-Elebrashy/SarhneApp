using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SarhneApp.Core.Entities;
using SarhneApp.Repository.Data;
using SarhneApp.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Repository.test.Repositories
{
    [TestFixture]
    internal class GenericRepositoryTests
    {
        private Message _messageObj;
        private DbContextOptions<SarhneDBContext> options;

        [SetUp]
        public void Setup()
        {
            options = new DbContextOptionsBuilder<SarhneDBContext>()
               .UseInMemoryDatabase(databaseName: "temp_Sarhne").Options;
            _messageObj = new Message
            {
                Id = 1,
                SenderId = null,
                ReceiverId = "raa-dd-bb",
                MessageText = "Hello",
                ImageUrl = "http://example.com/image.jpg",
                IsFavorite = false,
                IsAppeared = true,
                IsDeleted = false
            };
        }
        [Test]
        public async Task AddAsync_ShouldCallAddAsyncOnDbContext()
        {
            //act
            using (var context = new SarhneDBContext(options))
            {
                var repository = new GenericRepository<Message>(context);
                await repository.AddAsync(_messageObj);
                await context.SaveChangesAsync();
            }

            //assert
            using (var context = new SarhneDBContext(options))
            {
                var messageFromDb = context.Messages.FirstOrDefault(m => m.Id == 1);
                ClassicAssert.AreEqual(_messageObj.Id, messageFromDb.Id);
                ClassicAssert.AreEqual(_messageObj.SenderId, messageFromDb.SenderId);
                ClassicAssert.AreEqual(_messageObj.ReceiverId, messageFromDb.ReceiverId);
                ClassicAssert.AreEqual(_messageObj.MessageText, messageFromDb.MessageText);
                ClassicAssert.AreEqual(_messageObj.ImageUrl, messageFromDb.ImageUrl);
            }
        }
        [Test]
        public async Task GetAllAsync_ShouldReturnAllMessages()
        {
            // Arrange
            using (var context = new SarhneDBContext(options))
            {
                var repository = new GenericRepository<Message>(context);
                await repository.AddAsync(_messageObj);
                await context.SaveChangesAsync();
            }
            //Act 
            using (var context = new SarhneDBContext(options))
            {
                var repository = new GenericRepository<Message>(context);
                var messages = await repository.GetAllAsync();
                ClassicAssert.AreEqual(1, messages.Count);
                ClassicAssert.AreEqual(_messageObj.ReceiverId, messages.First().ReceiverId);
                ClassicAssert.AreEqual(_messageObj.MessageText, messages.First().MessageText);
                ClassicAssert.AreEqual(_messageObj.ImageUrl, messages.First().ImageUrl);
            }
        }
        [Test]
        public async Task GetAsync_ShouldReturnMessageById()
        {
            // Arrange
            // Arrange
            using (var context = new SarhneDBContext(options))
            {
                var repository = new GenericRepository<Message>(context);
                await repository.AddAsync(_messageObj);
                await context.SaveChangesAsync();
            }
            // Act
            using (var context = new SarhneDBContext(options))
            {
                var repository = new GenericRepository<SarhneApp.Core.Entities.Message>(context);
                var message = await repository.GetAsync(m => m.Id == 1);

                // Assert
                ClassicAssert.IsNotNull(message);
                ClassicAssert.AreEqual(_messageObj.MessageText, message.MessageText);
                ClassicAssert.AreEqual(_messageObj.ReceiverId, message.ReceiverId);
                ClassicAssert.AreEqual(_messageObj.MessageText, message.MessageText);
                ClassicAssert.AreEqual(_messageObj.ImageUrl, message.ImageUrl);
            }
        }
        [TearDown]
        public void Teardown()
        {
            using (var context = new SarhneDBContext(options))
            {
                context.Database.EnsureDeleted();
            }
        }
        
    }
}
