using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.ViewModels.Social.Friendship;

namespace Nillero.Core.Application.Mappings.DtosAndViewModels.Social
{
    public class FriendshipViewModelMappingProfile : Profile
    {
        public FriendshipViewModelMappingProfile()
        {
            // Requiere datos del usuario amigo
            // Se hizo manualmente porque FriendshipDto tiene User1Id y User2Id
            CreateMap<FriendshipDto, FriendViewModel>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.Ignore())
                .ForMember(dest => dest.ProfilePicture, opt => opt.Ignore())
                .ForMember(dest => dest.FriendshipCreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}
