using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.Entities
{
    public class Reaction
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReactionType { get; set; } // e.g., 'Like', 'Love', 'Care', 'Angry'

        // Navigation Property: This allows access to related UserReactions
        public ICollection<UserReaction> UserReactions { get; set; }
    }
}
