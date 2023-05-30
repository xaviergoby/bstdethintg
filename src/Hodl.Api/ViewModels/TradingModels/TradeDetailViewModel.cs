﻿namespace Hodl.Api.ViewModels.TradingModels;

public class TradeDetailViewModel
{
    public OrderListViewModel Order { get; set; }

    public string TransactionId { get; set; }

    public DateTime DateTime { get; set; }

    public decimal UnitPrice { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Executed { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Total { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Fee { get; set; }

    public CryptoCurrency FeeCurrency { get; set; }

    public string BookingPeriod { get; set; }
}
