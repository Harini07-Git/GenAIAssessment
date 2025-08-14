using System.Diagnostics.Metrics;

namespace ClaimsManagement.API.Services;

public class MetricsService
{
    private readonly Meter _meter;
    private readonly Counter<int> _requestCounter;
    private readonly Counter<int> _cacheHitCounter;
    private readonly Counter<int> _cacheMissCounter;
    private readonly Histogram<double> _responseTimeHistogram;

    public MetricsService()
    {
        _meter = new Meter("ClaimsAPI");
        _requestCounter = _meter.CreateCounter<int>("requests_total", "Number of requests processed");
        _cacheHitCounter = _meter.CreateCounter<int>("cache_hits_total", "Number of cache hits");
        _cacheMissCounter = _meter.CreateCounter<int>("cache_misses_total", "Number of cache misses");
        _responseTimeHistogram = _meter.CreateHistogram<double>("response_time_seconds", "Response times in seconds");
    }

    public void RecordRequest() => _requestCounter.Add(1);
    public void RecordCacheHit() => _cacheHitCounter.Add(1);
    public void RecordCacheMiss() => _cacheMissCounter.Add(1);
    public void RecordResponseTime(double seconds) => _responseTimeHistogram.Record(seconds);

    public Dictionary<string, object> GetMetrics()
    {
        return new Dictionary<string, object>
        {
            { "total_requests", _requestCounter.GetMetricPoints() },
            { "cache_hits", _cacheHitCounter.GetMetricPoints() },
            { "cache_misses", _cacheMissCounter.GetMetricPoints() },
            { "average_response_time", _responseTimeHistogram.GetMetricPoints() }
        };
    }
}
