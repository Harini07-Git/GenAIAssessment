using ClaimsManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ClaimsManagement.API.Services;

public class ClaimsService
{
    private readonly IMemoryCache _cache;
    private readonly MetricsService _metricsService;
    private readonly DbContext _dbContext;
    private const string CLAIMS_LIST_CACHE_KEY = "claims_list";
    private const string USER_DETAILS_CACHE_KEY = "user_details_{0}";
    private const string POLICY_METADATA_CACHE_KEY = "policy_metadata_{0}";

    public ClaimsService(IMemoryCache cache, MetricsService metricsService, DbContext dbContext)
    {
        _cache = cache;
        _metricsService = metricsService;
        _dbContext = dbContext;
    }

    public async Task<PaginatedResponse<Claim>> GetClaimsAsync(int pageNumber = 1, int pageSize = 10)
    {
        string cacheKey = $"{CLAIMS_LIST_CACHE_KEY}_{pageNumber}_{pageSize}";
        
        if (_cache.TryGetValue(cacheKey, out PaginatedResponse<Claim> cachedClaims))
        {
            _metricsService.RecordCacheHit();
            return cachedClaims;
        }

        _metricsService.RecordCacheMiss();

        var query = _dbContext.Set<Claim>()
            .AsNoTracking();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var claims = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new PaginatedResponse<Claim>
        {
            Items = claims,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        _cache.Set(cacheKey, response, cacheOptions);

        return response;
    }

    public async Task<Claim> GetClaimDetailsAsync(int claimId)
    {
        var claim = await _dbContext.Set<Claim>()
            .Include(c => c.Policy)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == claimId);

        return claim;
    }

    public void InvalidateClaimsCache()
    {
        _cache.Remove(CLAIMS_LIST_CACHE_KEY);
    }

    public void InvalidateUserCache(int userId)
    {
        _cache.Remove(string.Format(USER_DETAILS_CACHE_KEY, userId));
    }

    public void InvalidatePolicyCache(int policyId)
    {
        _cache.Remove(string.Format(POLICY_METADATA_CACHE_KEY, policyId));
    }
}
