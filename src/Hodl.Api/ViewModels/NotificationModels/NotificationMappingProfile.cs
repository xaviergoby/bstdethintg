namespace Hodl.Api.ViewModels.NotificationModels;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<NotificationMessage, NotificationDetailViewModel>();
        CreateMap<NotificationMessage, NotificationListViewModel>();
        CreateMap<PagingModel<NotificationMessage>, PagingViewModel<NotificationListViewModel>>();

        CreateMap<Notification, NotificationDetailViewModel>();
        CreateMap<Notification, NotificationListViewModel>();
        CreateMap<PagingModel<Notification>, PagingViewModel<NotificationListViewModel>>();
        CreateMap<NotificationEditViewModel, Notification>();
    }
}
