using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Domain.Entities.Social;

namespace Nillero.Core.Application.Mappings.DtosAndEntities.Social
{
    public class CommentMappingProfile : Profile
    {
        public CommentMappingProfile()
        {
            CreateMap<Comment, CommentDto>()
                .ReverseMap()
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
                .ForMember(dest => dest.RootComment, opt => opt.Ignore()) 
                .ForMember(dest => dest.Replies, opt => opt.Ignore());

        }
    }
}
