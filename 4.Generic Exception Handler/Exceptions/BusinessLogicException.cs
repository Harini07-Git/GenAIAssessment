namespace GenericExceptionHandler.Exceptions;

/// <summary>
/// Exception thrown when a business logic rule is violated
/// </summary>
public class BusinessLogicException : Exception
{
    public BusinessLogicException(string message) : base(message)
    {
    }
}
