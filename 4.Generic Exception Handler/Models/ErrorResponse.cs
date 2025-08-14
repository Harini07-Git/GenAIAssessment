namespace GenericExceptionHandler.Models;

/// <summary>
/// Represents a standardized error response
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public List<string> Details { get; set; }

    public ErrorResponse(int statusCode, string message, List<string> details = null)
    {
        StatusCode = statusCode;
        Message = message;
        Details = details ?? new List<string>();
    }
}
