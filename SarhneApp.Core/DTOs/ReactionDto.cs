using SarhneApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.DTOs
{
    public class ReactionDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReactionType { get; set; } // e.g., 'Like', 'Love', 'Care', 'Angry'
    }
}
