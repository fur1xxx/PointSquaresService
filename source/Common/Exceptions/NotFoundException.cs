namespace Common.Exceptions;

public class NotFoundException(string message, string errorCode) : ApiException(message, errorCode);
