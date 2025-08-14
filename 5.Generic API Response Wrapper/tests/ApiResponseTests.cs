using System;
using System.Collections.Generic;
using Xunit;

namespace GenericApiWrapper.Tests
{
    public class ApiResponseTests
    {
        [Fact]
        public void SuccessResponse_WithValidData_ShouldCreateSuccessResponse()
        {
            // Arrange
            var data = new List<string> { "test" };

            // Act
            var response = ApiResponse<List<string>>.SuccessResponse(data, "Success");

            // Assert
            Assert.True(response.Success);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("Success", response.Message);
            Assert.Equal(data, response.Data);
            Assert.NotNull(response.RequestId);
            Assert.Null(response.ErrorCode);
            Assert.Null(response.Pagination);
        }

        [Fact]
        public void ErrorResponse_WithValidData_ShouldCreateErrorResponse()
        {
            // Arrange & Act
            var response = ApiResponse<object>.ErrorResponse("NOT_FOUND", "Resource not found", 404);

            // Assert
            Assert.False(response.Success);
            Assert.Equal(404, response.StatusCode);
            Assert.Equal("Resource not found", response.Message);
            Assert.Equal("NOT_FOUND", response.ErrorCode);
            Assert.NotNull(response.RequestId);
            Assert.Null(response.Data);
            Assert.Null(response.Pagination);
        }

        [Fact]
        public void PaginatedResponse_WithValidData_ShouldCreatePaginatedResponse()
        {
            // Arrange
            var data = new List<string> { "item1", "item2" };

            // Act
            var response = ApiResponse<List<string>>.PaginatedResponse(
                data, 1, 2, 10, 5, "Paginated data");

            // Assert
            Assert.True(response.Success);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("Paginated data", response.Message);
            Assert.Equal(data, response.Data);
            Assert.NotNull(response.RequestId);
            Assert.NotNull(response.Pagination);
            Assert.Equal(1, response.Pagination.PageNumber);
            Assert.Equal(2, response.Pagination.PageSize);
            Assert.Equal(10, response.Pagination.TotalItems);
            Assert.Equal(5, response.Pagination.TotalPages);
        }

        [Fact]
        public void SuccessResponse_WithNullData_ShouldCreateValidResponse()
        {
            // Arrange & Act
            var response = ApiResponse<object>.SuccessResponse(null, "Success with null data");

            // Assert
            Assert.True(response.Success);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("Success with null data", response.Message);
            Assert.Null(response.Data);
            Assert.NotNull(response.RequestId);
            Assert.Null(response.ErrorCode);
            Assert.Null(response.Pagination);
        }
    }
}
