using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.DTOs
{
    public class ReactToMessageDto
    {
        public MessageAppearedDto Message { get; set; }
        public ReactionDto Reaction { get; set; }
    }
}
