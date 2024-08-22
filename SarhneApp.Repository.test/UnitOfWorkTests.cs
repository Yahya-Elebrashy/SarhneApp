using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SarhneApp.Core.Entities;
using SarhneApp.Repository.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Repository.test
{
    [TestFixture]
    internal class UnitOfWorkTests
    {
        private Message _messageObj;
        private DbContextOptions<SarhneDBContext> options;
        [SetUp]
        public void Setup()
        {
            options = new DbContextOptionsBuilder<SarhneDBContext>()
                .UseInMemoryDatabase(databaseName: "temp_Sarhne").Options;
            _messageObj = new Message()
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
        public async Task CompleteAsync_ShouldCallSaveChangesAsyncOnDbContext()
        {
            //act
            using (var context = new SarhneDBContext(options))
            {
                context.Database.EnsureDeleted();
                var unitOfWork = new UnitOfWork(context);
                await unitOfWork.Repositry<Message>().AddAsync(_messageObj);
                await unitOfWork.CompleteAsync();
            }

            //assert
            using (var context = new SarhneDBContext(options))
            {
                var messageFromDb = context.Messages.FirstOrDefault(m => m.Id == 1);
                ClassicAssert.IsNotNull(messageFromDb);
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
