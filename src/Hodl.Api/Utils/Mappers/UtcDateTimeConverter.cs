namespace Hodl.Api.Utils;

public class UtcDateTimeConverter : ITypeConverter<DateTime, DateTime>
{
    public DateTime Convert(DateTime source, DateTime destination, ResolutionContext context)
    {
        return source.ToUniversalTime();
    }
}

public class UtcDateTimeConverter2 : ITypeConverter<DateTime?, DateTime?>
{
    public DateTime? Convert(DateTime? source, DateTime? destination, ResolutionContext context)
    {
        return source == null
            ? null
            : ((DateTime)source).ToUniversalTime();
    }
}
