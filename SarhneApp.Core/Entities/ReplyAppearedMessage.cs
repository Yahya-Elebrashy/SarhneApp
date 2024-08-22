using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.Entities
{
    public class ReplyAppearedMessage
    {
        public int Id { get; set; }

        #region Foreign key for the message this reply belongs to
        [ForeignKey(nameof(Message))]
        public int MessageId { get; set; }
        public Message Message { get; set; }
        #endregion

        // Content of the reply
        [StringLength(500)]
        public string ReplyText { get; set; }

        // Date and time when the reply was created
        public DateTime DateOfCreation { get; set; } = DateTime.UtcNow;
    }
}
