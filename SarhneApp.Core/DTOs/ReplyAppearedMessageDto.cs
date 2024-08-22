using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.DTOs
{
    public class ReplyAppearedMessageDto
    {
        public int Id { get; set; }

        public string ReplyText { get; set; }
        public DateTime DateOfCreation { get; set; }
        public MessageAppearedDto Message { get; set; }



    }
}
