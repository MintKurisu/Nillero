using AutoMapper;
using Nillero.Core.Application.Dtos.Notifications;
using Nillero.Core.Application.ViewModels.Notifications;
using Nillero.Core.Domain.Entities.Notifications;

namespace Nillero.Core.Application.Mappings.DtosAndEntities.Notifications
{
    public class NotificationMappingProfile : Profile
    {
        public NotificationMappingProfile()
        {
            // Entity <-> DTO
            CreateMap<Notification, NotificationDto>();
            CreateMap<NotificationDto, Notification>();

            // DTO -> ViewModel
            CreateMap<NotificationDto, NotificationViewModel>()
                .ForMember(dest => dest.Link, opt => opt.MapFrom(src =>
                    src.PostId.HasValue
                        ? src.CommentId.HasValue
                            ? $"/Home/Details/{src.PostId}?commentId={src.CommentId}#comment-{src.CommentId}"
                            : $"/Home/Details/{src.PostId}"
                        : string.Empty));
        }
    }
}
