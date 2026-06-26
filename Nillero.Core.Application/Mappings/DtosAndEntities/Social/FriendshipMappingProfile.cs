using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Domain.Entities.Social;

namespace Nillero.Core.Application.Mappings.DtosAndEntities.Social
{
    public class FriendshipMappingProfile : Profile
    {
        public FriendshipMappingProfile()
        {
            CreateMap<Friendship, FriendshipDto>()
                .ReverseMap();

        }
    }
}
