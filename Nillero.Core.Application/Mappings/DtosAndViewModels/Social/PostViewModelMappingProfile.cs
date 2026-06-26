using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.ViewModels.Social.Posts;
using Nillero.Core.Domain.Common.Enum.Social;

namespace Nillero.Core.Application.Mappings.DtosAndViewModels.Social
{
    public class PostViewModelMappingProfile : Profile
    {
        public PostViewModelMappingProfile()
        {
            CreateMap<PostDto, SavePostViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int?)src.Id))
                .ForMember(dest => dest.MediaType, opt => opt.MapFrom(src =>
                    src.Type == PostType.Image ? "Imagen" : "Video"))
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? 0))
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src =>
                    src.MediaType == "Imagen" ? PostType.Image : PostType.Video))
                .ForMember(dest => dest.MediaPath, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Requiere datos adicionales que no estan en PostDto
            CreateMap<PostDto, PostViewModel>()
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.UserFullName, opt => opt.Ignore())
                .ForMember(dest => dest.UserProfilePicture, opt => opt.Ignore())
                .ForMember(dest => dest.IsOwner, opt => opt.Ignore())
                .ForMember(dest => dest.LikeCount, opt => opt.Ignore())
                .ForMember(dest => dest.DislikeCount, opt => opt.Ignore())
                .ForMember(dest => dest.UserReaction, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore());
        }
    }
}
