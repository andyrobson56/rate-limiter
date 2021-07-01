using AirTasker_RateLimiter.Interfaces;

namespace AirTasker_RateLimiter.Models
{
    public class RateLimitLevels : IRateLimitLevels
    {
        private int _seconds;
        private int _requests;

        public RateLimitLevels(int seconds, int requests)
        {
            _seconds  = seconds;
            _requests = requests;
        }

        public int Seconds => _seconds;

        public int Requests => _requests;
    }
}