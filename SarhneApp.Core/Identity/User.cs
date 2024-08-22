using Microsoft.AspNetCore.Identity;
using SarhneApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SarhneApp.Core.Identity
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Gender
    {
        Male,
        Female
    }
    public class User : IdentityUser
    {
        public Gender Gender { get; set; }

        [StringLength(50)]
        public string Link { get; set; }

        [StringLength(50)]
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string? DetailsAboutMe { get; set; }
        public DateTime DateOfCreation { get; set; } = DateTime.UtcNow;

        // Navigation property for the messages sent by this user
        public ICollection<Message> SentMessages { get; set; }

        // Navigation property for the messages received by this user
        public ICollection<Message> ReceivedMessages { get; set; }
        public List<RefreshToken>? RefreshTokens { get; set; }
    }
}
