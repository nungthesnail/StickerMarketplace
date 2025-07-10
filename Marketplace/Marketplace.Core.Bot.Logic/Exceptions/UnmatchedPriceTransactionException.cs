namespace Marketplace.Core.Bot.Logic.Exceptions;

public class UnmatchedPriceTransactionException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException);

