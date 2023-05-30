using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.ExternalAccountModels;

public class ExchangeListViewModel
{
    [Key]
    public Guid Id { get; set; }

    public string ExchangeName { get; set; }

    public string Url { get; set; }

    public bool IsDefi { get; set; }
}
