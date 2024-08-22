using AutoMapper;
using SarhneApp.Core.DTOs;
using SarhneApp.Core.Entities;
using SarhneApp.Core.Identity;

namespace SarhneApp.Api.Helper
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<SendMessageDto, Message>();
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.DateOfCreation, opt => opt.MapFrom(src => src.DateOfCreation.ToString("yyyy-MM-dd HH:mm:ss")));
            CreateMap<UserDto, User>().ReverseMap();
            CreateMap<AddReplyAppearedMessageDto,ReplyAppearedMessage>();
            CreateMap<ReplyAppearedMessage,ReplyAppearedMessageDto>();
            CreateMap<ReplyAppearedMessage, RepliesForAppearedMessageDto>();
            CreateMap<Message, MessageAppearedDto>();

            CreateMap<Reaction, ReactionDto>();
            CreateMap<UserReaction,ReactToMessageDto>();

        }
    }
}
