using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SarhneApp.Core.Entities;
using SarhneApp.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Repository.Data
{
    public class SarhneDBContext : IdentityDbContext<User>
    {
        public SarhneDBContext(DbContextOptions<SarhneDBContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
            .HasIndex(u => u.Link)
            .IsUnique();

            builder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired();

            builder.Entity<User>()
                .Property(u => u.UserName)
                .IsRequired();

            #region Message Entity
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Reply Appeared Message Entity
            // Configuring the relationship with cascade delete
            builder.Entity<ReplyAppearedMessage>()
                .HasOne(r => r.Message)
                .WithMany(m => m.AppearedReplies)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            builder.Entity<UserReaction>()
                   .HasKey(ur => new { ur.UserId, ur.MessageId });

            // Seeding some initial reactions (optional)
            builder.Entity<Reaction>().HasData(
                new Reaction { Id = 1, ReactionType = "Like" },
                new Reaction { Id = 2, ReactionType = "Love" },
                new Reaction { Id = 3, ReactionType = "Care" },
                new Reaction { Id = 4, ReactionType = "Angry" }
            );
        }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ReplyAppearedMessage> ReplyAppearedMessages { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<UserReaction> UserReactions { get; set; }
    }
}
