using System.Text;
using ClaimsManagement.API.Models;
using ClaimsManagement.API.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace ClaimsManagement.API.Tests;

public class ClaimsServiceTests
{
    [Fact]
    public async Task GetClaimsAsync_CacheHit_ReturnsCachedData()
    {
        // Arrange
        var mockCache = new Mock<IMemoryCache>();
        var mockMetrics = new Mock<MetricsService>();
        var mockDbContext = new Mock<DbContext>();

        var cachedResponse = new PaginatedResponse<Claim>
        {
            Items = new List<Claim> { new Claim { Id = 1 } },
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 1,
            TotalPages = 1
        };

        mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedResponse))
            .Returns(true);

        var service = new ClaimsService(mockCache.Object, mockMetrics.Object, mockDbContext.Object);

        // Act
        var result = await service.GetClaimsAsync();

        // Assert
        mockMetrics.Verify(x => x.RecordCacheHit(), Times.Once);
        Assert.Equal(cachedResponse, result);
    }

    [Fact]
    public void GetMetrics_ReturnsCorrectFormat()
    {
        // Arrange
        var metricsService = new MetricsService();

        // Act
        var metrics = metricsService.GetMetrics();

        // Assert
        Assert.Contains("total_requests", metrics.Keys);
        Assert.Contains("cache_hits", metrics.Keys);
        Assert.Contains("cache_misses", metrics.Keys);
        Assert.Contains("average_response_time", metrics.Keys);
    }

    [Fact]
    public async Task GetClaimsAsync_ValidPagination_ReturnsCorrectPage()
    {
        // Arrange
        var mockCache = new Mock<IMemoryCache>();
        var mockMetrics = new Mock<MetricsService>();
        var mockDbContext = new Mock<DbContext>();

        var claims = Enumerable.Range(1, 25)
            .Select(i => new Claim { Id = i })
            .ToList();

        var mockSet = new Mock<DbSet<Claim>>();
        mockSet.As<IQueryable<Claim>>()
            .Setup(m => m.Provider)
            .Returns(claims.AsQueryable().Provider);

        mockSet.As<IQueryable<Claim>>()
            .Setup(m => m.Expression)
            .Returns(claims.AsQueryable().Expression);

        mockSet.As<IQueryable<Claim>>()
            .Setup(m => m.ElementType)
            .Returns(claims.AsQueryable().ElementType);

        mockSet.As<IQueryable<Claim>>()
            .Setup(m => m.GetEnumerator())
            .Returns(claims.AsQueryable().GetEnumerator());

        mockDbContext.Setup(x => x.Set<Claim>())
            .Returns(mockSet.Object);

        var service = new ClaimsService(mockCache.Object, mockMetrics.Object, mockDbContext.Object);

        // Act
        var result = await service.GetClaimsAsync(pageNumber: 2, pageSize: 10);

        // Assert
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
    }
}
