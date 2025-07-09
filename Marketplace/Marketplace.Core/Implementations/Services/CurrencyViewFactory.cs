using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Implementations.Services;

public class CurrencyViewFactory : ICurrencyViewFactory
{
    public string CreateView(TransactionMethod transactionMethod)
    {
        return transactionMethod switch
        {
            TransactionMethod.TelegramStars => "*",
            _ => throw new ArgumentOutOfRangeException(nameof(transactionMethod), transactionMethod, null)
        };
    }
}
