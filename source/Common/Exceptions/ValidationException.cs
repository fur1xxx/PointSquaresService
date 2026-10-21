namespace Common.Exceptions;

public class ValidationException(string message, string errorCode) : ApiException(message, errorCode);
