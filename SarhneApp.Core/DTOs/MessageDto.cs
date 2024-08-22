using SarhneApp.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SarhneApp.Core.Entities;

namespace SarhneApp.Core.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }

        public UserDto Sender { get; set; }

        public UserDto Receiver { get; set; }

        public string MessageText { get; set; }

        public string ImageUrl { get; set; }

        public bool IsFavorite { get; set; }
        public bool IsSecretly { get; set; }

        public bool IsAppeared { get; set; }

        public string DateOfCreation { get; set; }
        public ICollection<RepliesForAppearedMessageDto> AppearedReplies { get; set; }
        public ICollection<ReactionCountDto> ReactionCounts { get; set; } = new List<ReactionCountDto>();
    }
}
