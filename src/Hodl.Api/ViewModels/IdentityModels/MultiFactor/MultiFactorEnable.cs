namespace Hodl.Api.ViewModels.IdentityModels.MultiFactor;

public record MultiFactorEnable
{
    public string Key { get; set; }

    public string QrImgSrc { get; set; }
}
