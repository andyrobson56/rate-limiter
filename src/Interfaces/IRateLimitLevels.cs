namespace AirTasker_RateLimiter.Interfaces
{
    public interface IRateLimitLevels
    {
        int Seconds { get; }
        int Requests { get; }
    }
}