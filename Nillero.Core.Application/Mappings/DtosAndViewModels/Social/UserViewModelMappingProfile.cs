using AutoMapper;
using Nillero.Core.Application.Dtos.User;
using Nillero.Core.Application.ViewModels.Social.FriendRequest;
using Nillero.Core.Application.ViewModels.Social.Friendship;
using Nillero.Core.Application.ViewModels.Social.User;

namespace Nillero.Core.Application.Mappings.DtosAndViewModels.Social
{
    public class UserViewModelMappingProfile : Profile
    {
        public UserViewModelMappingProfile()
        {
            CreateMap<UserDto, AvailableUserViewModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.ProfilePicturePath))
                .ForMember(dest => dest.MutualFriendsCount, opt => opt.Ignore());

            CreateMap<UserDto, FriendViewModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.ProfilePicturePath))
                .ForMember(dest => dest.FriendshipCreatedAt, opt => opt.Ignore());

            CreateMap<UserDto, EditProfileViewModel>()
                .ForMember(dest => dest.CurrentProfilePicturePath, opt => opt.MapFrom(src => src.ProfilePicturePath))
                .ForMember(dest => dest.ProfilePicture, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore());

            CreateMap<EditProfileViewModel, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.ProfilePicturePath, opt => opt.MapFrom(src => src.CurrentProfilePicturePath))
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            CreateMap<ViewModels.Login.RegisterViewModel, UserDto>() 
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProfilePicturePath, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => false));
        }
    }

}
