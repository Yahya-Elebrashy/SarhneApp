using SarhneApp.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.Entities
{
    public class Message
    {
        public int Id { get; set; }

        #region Foreign key for the sender of the message
        [ForeignKey(nameof(Sender))]
        public string? SenderId { get; set; }
        public User Sender { get; set; }
        #endregion

        #region Foreign key for the receiver of the message
        [ForeignKey(nameof(Receiver))]
        public string ReceiverId { get; set; }
        public User Receiver { get; set; }
        #endregion

        // Content of the message
        [StringLength(500)]
        public string? MessageText { get; set; }

        // Url of the Image
        public string? ImageUrl { get; set; }

        // Indicates if the message is marked as favorite
        public bool IsFavorite { get; set; }
        // Indicates if the message is marked as Secretly
        public bool IsSecretly { get; set; }

        // Indicates if the message has appeared to the user
        public bool IsAppeared { get; set; }

        // Indicates if the message has deleted to the user
        public bool IsDeleted { get; set; }

        // Date and time when the message was created
        public DateTime DateOfCreation { get; set; } = DateTime.UtcNow;

        // Collection of replies associated with the message
        public ICollection<ReplyAppearedMessage> AppearedReplies { get; set; } = new List<ReplyAppearedMessage>();

        // Collection of Reactions associated to message
        public ICollection<UserReaction> UserReactions { get; set; } = new List<UserReaction>();
    }
}
