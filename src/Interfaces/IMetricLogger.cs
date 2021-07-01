namespace AirTasker_RateLimiter.Interfaces
{
    /// IMetricLogger
    ///
    /// Interface representing the ability to log custom metrics about the running application
    public interface IMetricLogger
    {
        const int RequestRateLimited = 0;
        void LogMetric(int metricId, string tag);
    }
}