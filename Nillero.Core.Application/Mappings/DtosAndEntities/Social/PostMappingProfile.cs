using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Domain.Entities.Social;

namespace Nillero.Core.Application.Mappings.DtosAndEntities.Social
{
    public class PostMappingProfile : Profile
    {
        public PostMappingProfile()
        {
            CreateMap<Post, PostDto>()
                .ReverseMap()
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Reactions, opt => opt.Ignore());
        }
    }
}
