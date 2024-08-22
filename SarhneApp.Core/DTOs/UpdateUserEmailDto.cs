using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.DTOs
{
    public class UpdateUserEmailDto
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }
}
