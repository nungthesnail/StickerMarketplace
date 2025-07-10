using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Implementations.Services;

public class CurrencyViewFactory : ICurrencyViewFactory
{
    public string CreateView(TransactionCurrency transactionMethod)
    {
        return transactionMethod switch
        {
            TransactionCurrency.Xtr => "*",
            _ => throw new ArgumentOutOfRangeException(nameof(transactionMethod), transactionMethod, null)
        };
    }
}
