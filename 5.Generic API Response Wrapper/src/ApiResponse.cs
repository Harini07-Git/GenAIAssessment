using System;
using System.Text.Json.Serialization;

namespace GenericApiWrapper
{
    /// <summary>
    /// Generic wrapper class for standardized API responses.
    /// </summary>
    /// <typeparam name="T">The type of data being wrapped in the response.</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Gets or sets the timestamp of when the response was created (in UTC).
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for this request.
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the error code (if any).
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Gets or sets the pagination information (if applicable).
        /// </summary>
        public PaginationMetadata? Pagination { get; set; }

        /// <summary>
        /// Creates a success response with the specified data and message.
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string message = "Request successful", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow,
                RequestId = Guid.NewGuid().ToString()
            };
        }

        /// <summary>
        /// Creates an error response with the specified error details.
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string errorCode, string message, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                ErrorCode = errorCode,
                Message = message,
                Timestamp = DateTime.UtcNow,
                RequestId = Guid.NewGuid().ToString()
            };
        }

        /// <summary>
        /// Creates a paginated success response with the specified data and pagination information.
        /// </summary>
        public static ApiResponse<T> PaginatedResponse(
            T data,
            int pageNumber,
            int pageSize,
            int totalItems,
            int totalPages,
            string message = "Request successful",
            int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow,
                RequestId = Guid.NewGuid().ToString(),
                Pagination = new PaginationMetadata
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
        }
    }

    /// <summary>
    /// Represents pagination metadata for API responses.
    /// </summary>
    public class PaginationMetadata
    {
        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the size of each page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of items available.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages available.
        /// </summary>
        public int TotalPages { get; set; }
    }
}
