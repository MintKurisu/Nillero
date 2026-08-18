using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.ViewModels.Social.Comment;

namespace Nillero.Core.Application.Mappings.DtosAndViewModels.Social
{
    public class CommentViewModelMappingProfile : Profile
    {
        public CommentViewModelMappingProfile()
        {
            CreateMap<CommentDto, SaveCommentViewModel>()
                 .ReverseMap()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? 0))
                 .ForMember(dest => dest.UserId, opt => opt.Ignore())
                 .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                 .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<CommentDto, CommentViewModel>()
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.UserFullName, opt => opt.Ignore())
                .ForMember(dest => dest.UserProfilePicture, opt => opt.Ignore())
                .ForMember(dest => dest.IsOwner, opt => opt.Ignore());

        }
    }
}
