using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.DTOs
{
    public class UpdateReplyAppearedMessageDto
    {
        [Required]
        [StringLength(500)]
        public string ReplyText { get; set; }
    }
}
