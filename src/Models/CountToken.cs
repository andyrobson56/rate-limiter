using System;

namespace AirTasker_RateLimiter.Models
{
    /// RequestCountToken
    ///
    /// Represents a token that records the number of requests received since
    /// StartTime
    public class RequestCountToken
    {
        public RequestCountToken(DateTime startTime, int requestCount)
        {
            StartTime = startTime;
            RequestCount = requestCount;
        }

        public DateTime StartTime { get; set; }
        public int RequestCount{ get; set; }

        public static RequestCountToken NewToken
        { 
            get 
            { 
                return new RequestCountToken(DateTime.UtcNow, 1); 
            }
        }
    }
}