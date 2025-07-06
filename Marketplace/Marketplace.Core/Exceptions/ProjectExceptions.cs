namespace Marketplace.Core.Exceptions;

public class ProjectNameAlreadyExistsException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException);


public class ProjectUrlAlreadyExistsException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException);
