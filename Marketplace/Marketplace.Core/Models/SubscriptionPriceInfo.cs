using Marketplace.Core.Implementations.Services;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Models;

public record SubscriptionPriceInfo(
    int Price,
    TransactionCurrency Currency,
    double DayCount,
    bool Enhanced)
{
    public string GetAsLabel()
    {
        var currencyViewFactory = new CurrencyViewFactory();
        var currencyView = currencyViewFactory.CreateView(Currency);
        return $"{DayCount:F0} дней - {Price} {currencyView}";
    }
}
