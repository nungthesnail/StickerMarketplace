namespace Marketplace.Core.Exceptions;

public class UserAlreadyHaveSubscriptionException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException);
