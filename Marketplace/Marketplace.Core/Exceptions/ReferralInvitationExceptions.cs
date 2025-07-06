namespace Marketplace.Core.Exceptions;

public class UserAlreadyInvitedException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException);
