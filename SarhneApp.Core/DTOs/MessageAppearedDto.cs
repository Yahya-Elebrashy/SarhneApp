using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.DTOs
{
    public class MessageAppearedDto
    {
        public int Id { get; set; }

        public UserDto Sender { get; set; }

        public string MessageText { get; set; }

        public string ImageUrl { get; set; }

        public bool IsSecretly { get; set; }

        public string DateOfCreation { get; set; }
    }
}
