using ClaimsManagement.API.Models;
using ClaimsManagement.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ClaimsManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly ClaimsService _claimsService;
    private readonly MetricsService _metricsService;

    public ClaimsController(ClaimsService claimsService, MetricsService metricsService)
    {
        _claimsService = claimsService;
        _metricsService = metricsService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<Claim>>> GetClaims(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var stopwatch = Stopwatch.StartNew();
        _metricsService.RecordRequest();

        try
        {
            var claims = await _claimsService.GetClaimsAsync(pageNumber, pageSize);
            return Ok(claims);
        }
        finally
        {
            stopwatch.Stop();
            _metricsService.RecordResponseTime(stopwatch.Elapsed.TotalSeconds);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Claim>> GetClaimById(int id)
    {
        var stopwatch = Stopwatch.StartNew();
        _metricsService.RecordRequest();

        try
        {
            var claim = await _claimsService.GetClaimDetailsAsync(id);
            if (claim == null)
                return NotFound();

            return Ok(claim);
        }
        finally
        {
            stopwatch.Stop();
            _metricsService.RecordResponseTime(stopwatch.Elapsed.TotalSeconds);
        }
    }

    [HttpPost("invalidate-cache")]
    public IActionResult InvalidateCache()
    {
        _claimsService.InvalidateClaimsCache();
        return Ok();
    }

    [HttpGet("metrics")]
    public IActionResult GetMetrics()
    {
        var metrics = _metricsService.GetMetrics();
        return Ok(metrics);
    }
}
