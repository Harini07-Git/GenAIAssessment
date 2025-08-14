using System.Net;
using System.Text.Json;
using GenericExceptionHandler.Exceptions;
using GenericExceptionHandler.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GenericExceptionHandler.Tests;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        _envMock = new Mock<IWebHostEnvironment>();
        _httpContext = new DefaultHttpContext();
    }

    [Fact]
    public async Task HandleValidationException_ReturnsBadRequest()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2" };
        var middleware = CreateMiddleware(new ValidationException("Validation failed", errors));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
        var response = await GetResponseAsType<ErrorResponse>(_httpContext);
        Assert.Equal(errors, response.Details);
    }

    [Fact]
    public async Task HandleBusinessLogicException_ReturnsConflict()
    {
        // Arrange
        var message = "Business logic error";
        var middleware = CreateMiddleware(new BusinessLogicException(message));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.Conflict, _httpContext.Response.StatusCode);
        var response = await GetResponseAsType<ErrorResponse>(_httpContext);
        Assert.Equal(message, response.Message);
    }

    [Fact]
    public async Task HandleNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var message = "Resource not found";
        var middleware = CreateMiddleware(new NotFoundException(message));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, _httpContext.Response.StatusCode);
        var response = await GetResponseAsType<ErrorResponse>(_httpContext);
        Assert.Equal(message, response.Message);
    }

    [Fact]
    public async Task HandleUnknownException_ReturnsInternalServerError()
    {
        // Arrange
        var middleware = CreateMiddleware(new Exception("Unknown error"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
        var response = await GetResponseAsType<ErrorResponse>(_httpContext);
        Assert.Equal("An unexpected error occurred", response.Message);
    }

    private GlobalExceptionHandlerMiddleware CreateMiddleware(Exception exception)
    {
        RequestDelegate next = (HttpContext httpContext) => throw exception;
        return new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object, _envMock.Object);
    }

    private async Task<T> GetResponseAsType<T>(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var jsonResponse = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<T>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
