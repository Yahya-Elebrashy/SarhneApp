using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SarhneApp.Core.Identity;

namespace SarhneApp.Core.Entities
{
    public class UserReaction
    {
        [Required]
        public string UserId { get; set; }
        // Navigation Property: Link to the User entity
        public User User { get; set; }

        [Required]
        public int MessageId { get; set; } 
        public Message Message { get; set; }

        [Required]
        public int ReactionId { get; set; } 

        // Navigation Property: Link to the Reaction entity
        [ForeignKey("ReactionId")]
        public Reaction Reaction { get; set; }

        [Required]
        public DateTime ReactionDateTime { get; set; } = DateTime.Now;

    }
}
