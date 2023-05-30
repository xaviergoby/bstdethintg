using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.ExternalAccountModels;

public class BankListViewModel
{
    [Key]
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string BIC { get; set; }

    public string City { get; set; }

    public string Branch { get; set; }

    public string Address { get; set; }

    public string Zipcode { get; set; }

    public string Country { get; set; }

    public string CountryCode { get; set; }

    public string Url { get; set; }
}
