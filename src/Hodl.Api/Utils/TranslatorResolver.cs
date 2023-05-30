using Microsoft.Extensions.Localization;

namespace Hodl.Api.Utils;

public class TranslatorResolver : IMemberValueResolver<object, object, string, string>
{
    private readonly IStringLocalizer<TranslatorResolver> _localizer;

    public TranslatorResolver(IStringLocalizer<TranslatorResolver> localizer)
    {
        _localizer = localizer;
    }

    public string Resolve(object source, object destination, string sourceMember, string destMember, ResolutionContext context)
    {
        return _localizer[sourceMember];
    }
}
