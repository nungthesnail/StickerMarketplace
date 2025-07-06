namespace Marketplace.Core.Exceptions;

public class TagAlreadyExistsException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException);
