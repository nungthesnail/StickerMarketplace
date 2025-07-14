namespace Marketplace.Core.Bot.Logic.Exceptions;

public class UnknownCategoryException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException);
