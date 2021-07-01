

namespace AirTasker_RateLimiter.Interfaces
{
    /// IRequest
    ///
    /// Simplified interface representing the http request received.
    /// In dotnet core this would typically be an httpContext.
    public interface IRequest
    {
        /// Identifier of the request
        string Id();
    }
}