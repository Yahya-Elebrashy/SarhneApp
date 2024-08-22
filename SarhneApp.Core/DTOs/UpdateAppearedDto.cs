using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.DTOs
{
    public class UpdateAppearedDto
    {
        [Required]
        public bool IsAppeared { get; set; }
    }
}
