using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.ViewModels.Social.FriendRequest;

namespace LinkUpApp.Core.Application.Mappings.DtosAndViewModels.Social
{
    public class FriendRequestViewModelMappingProfile : Profile
    {
        public FriendRequestViewModelMappingProfile()
        {
            // Requiere datos del usuario sender
            CreateMap<FriendRequestDto, PendingFriendRequestViewModel>()
                .ForMember(dest => dest.SenderUserName, opt => opt.Ignore())
                .ForMember(dest => dest.SenderFullName, opt => opt.Ignore())
                .ForMember(dest => dest.SenderProfilePicture, opt => opt.Ignore())
                .ForMember(dest => dest.MutualFriendsCount, opt => opt.Ignore());

            // Requiere datos del usuario receiver
            CreateMap<FriendRequestDto, SentFriendRequestViewModel>()
                .ForMember(dest => dest.ReceiverUserName, opt => opt.Ignore())
                .ForMember(dest => dest.ReceiverFullName, opt => opt.Ignore())
                .ForMember(dest => dest.ReceiverProfilePicture, opt => opt.Ignore())
                .ForMember(dest => dest.MutualFriendsCount, opt => opt.Ignore());
        }
    }
}
