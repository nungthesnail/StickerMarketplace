namespace Marketplace.Core.Exceptions;

public class PromocodeAlreadyExistsException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException);
