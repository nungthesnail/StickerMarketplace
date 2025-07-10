namespace Marketplace.Core.Bot.Logic.Exceptions;

public class UndefinedTransactionPurposeException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException);
