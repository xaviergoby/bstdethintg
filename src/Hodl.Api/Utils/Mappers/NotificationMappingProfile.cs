namespace Hodl.Api.Utils.Mappers;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<Notification, NotificationMessage>();
    }
}
