using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.ExternalAccountModels;

public class ExchangeEditViewModel
{
    [MaxLength(60)]
    public string ExchangeName { get; set; }

    [MaxLength(256)]
    public string Url { get; set; }

    public string Icon { get; set; }

    public bool IsDefi { get; set; }
}
