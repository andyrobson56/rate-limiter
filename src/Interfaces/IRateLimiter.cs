using System.Threading.Tasks;

namespace AirTasker_RateLimiter.Interfaces
{
    /// IRateLimiter
    ///
    /// Interface for the main service, the RateLimiter
    public interface IRateLimiter
    {
        Task<(int ReturnCode, string Message)> IsRequestRateLimitedAsync(IRequest request);
    }
}