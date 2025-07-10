using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Abstractions.Services;

public interface ICurrencyViewFactory
{
    string CreateView(TransactionCurrency transactionMethod);
}
