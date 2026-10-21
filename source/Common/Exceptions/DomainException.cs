namespace Common.Exceptions;

public class DomainException(string message, string errorCode) : ApiException(message, errorCode);
