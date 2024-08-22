using SarhneApp.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.DTOs
{
    public class UpdateUserDataDto
    {
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public string Name { get; set; }
        public string? DetailsAboutMe { get; set; }

    }
}
