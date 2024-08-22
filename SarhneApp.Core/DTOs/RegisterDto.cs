using Microsoft.AspNetCore.Http;
using SarhneApp.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SarhneApp.Core.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender Gender { get; set; }

        [StringLength(50)]
        public string? Link { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public IFormFile Image { get; set; }

        [StringLength(500)]
        public string? DetaisAboutMe { get; set; }
    }
}
