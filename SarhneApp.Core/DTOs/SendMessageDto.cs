using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.DTOs
{
    public class SendMessageDto
    {
        [Required]
        public string ReceiverId { get; set; }
        [StringLength(500)]
        public string? MessageText { get; set; }
        public IFormFile? Image { get; set; }
        public bool IsSecretly { get; set; } = true;


    }
}
