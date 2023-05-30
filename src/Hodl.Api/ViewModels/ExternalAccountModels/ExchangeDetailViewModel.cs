namespace Hodl.Api.ViewModels.ExternalAccountModels;

public class ExchangeDetailViewModel
{
    public string ExchangeName { get; set; }

    public string Url { get; set; }

    public byte[] Icon { get; set; }

    public bool IsDefi { get; set; }

    public virtual ICollection<ExchangeAccountListViewModel> ExchangeAccounts { get; set; }
}
