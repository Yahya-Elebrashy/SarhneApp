using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.DTOs
{
    public class AddReactToMessageDto
    {
        [Required]
        public int MessageId { get; set; }
        [Required]
        public int ReactionId { get; set; }
    }
}
