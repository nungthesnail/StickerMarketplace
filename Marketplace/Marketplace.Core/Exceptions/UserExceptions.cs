namespace Marketplace.Core.Exceptions;

public class UserNameAlreadyExistsException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException);

public class UserIdAlreadyExistsException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException);
