using Hodl.ExchangeAPI.Models;

namespace Hodl.Api.Utils.Mappers;

public class ExchangeApiMappingProfile : Profile
{
    public ExchangeApiMappingProfile()
    {
        CreateMap<ExchangeOrder, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.BaseAsset, opt => opt.Ignore())
            .ForMember(dest => dest.QuoteAsset, opt => opt.Ignore())
            .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => (OrderDirection)src.Direction))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => (OrderState)src.State))
            .ForMember(dest => dest.ExchangeOrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.ExchangeClientOrderId, opt => opt.MapFrom(src => src.ClientOrderId))
            .ForMember(dest => dest.OrderNumber, opt => opt.Ignore());

        CreateMap<ExchangeTrade, Trade>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.FeeCurrency, opt => opt.Ignore())
            .ForMember(dest => dest.ExchangeTradeId, opt => opt.MapFrom(src => src.Id));
    }
}
