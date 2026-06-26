using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Domain.Entities.Social;

namespace Nillero.Core.Application.Mappings.DtosAndEntities.Social
{
    public class PostReactionMappingProfile : Profile
    {
        public PostReactionMappingProfile()
        {
            CreateMap<PostReaction, PostReactionDto>()
                .ReverseMap()
                .ForMember(dest => dest.Post, opt => opt.Ignore());
        }
    }
}
