using System;
using System.Threading;
using System.Threading.Tasks;
using AirTasker_RateLimiter.Interfaces;
using AirTasker_RateLimiter.Models;
using Serilog;

namespace AirTasker_RateLimiter.Services
{
    /// RateLimiter
    ///
    /// Main service offering clients the ability to rate limit requests 
    public class RateLimiter : IRateLimiter
    {
        private static SemaphoreSlim semaphore;
        private const int concurrentRequestLimit = 1;

        private readonly int _requests;
        private readonly int _seconds;
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger _logger;
        private readonly IMetricLogger _metricLogger;

        /// requests - number of requests allowed
        /// seconds - time allowed for requests
        /// tokenRepository - repository for storing of count tokens by Id
        /// logger - log messages
        /// metricLogger - performance metrics
        public RateLimiter(int requests, int seconds, ITokenRepository tokenRepository, ILogger logger, IMetricLogger metricLogger)
        {
            _requests = requests;
            _seconds = seconds;
            _tokenRepository = tokenRepository;
            _logger = logger;
            _metricLogger = metricLogger;

            semaphore = new SemaphoreSlim(concurrentRequestLimit, concurrentRequestLimit);
        }

        /// GetRequestCountToken
        /// 
        /// Retrieve or create a RequestCountToken for the given tokenId
        protected async Task<RequestCountToken> GetRequestCountToken(string tokenId)
        {
            RequestCountToken countToken;

            // Make sure only one thread can access this
            await semaphore.WaitAsync();

            try
            {
                countToken = await _tokenRepository.GetTokenByIdAsync(tokenId);

                // do we have an unexpired token
                if (countToken?.StartTime.AddSeconds(_seconds) >= DateTime.UtcNow)
                {
                    countToken = new RequestCountToken(countToken.StartTime, countToken.RequestCount + 1);
                }
                // either no token, or it is expired
                else
                {
                    countToken = RequestCountToken.NewToken;
                }

                await _tokenRepository.PutToken(tokenId, countToken);
            }
            finally
            {
                semaphore.Release();
            }

            return countToken;
        }

        /// IsRequestRateLimitedAsync
        /// 
        /// Check if request is rate limited
        public async Task<(int ReturnCode, string Message)> IsRequestRateLimitedAsync(IRequest request)
        {
            var id = request.Id();

            try
            {
                var requestCountToken = await GetRequestCountToken(id);

                if (requestCountToken.RequestCount <= _requests)
                {
                    return (200, string.Empty);
                }

                _metricLogger.LogMetric(IMetricLogger.RequestRateLimited, "Request for id {id} rate limited");

                var secondsToWait = (long)(DateTime.UtcNow - requestCountToken.StartTime).TotalSeconds;

                return (429, $"Rate limit exceeded. Try again in #{secondsToWait} seconds");
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "Error getting CountToken for id {id}.");
                
                throw new Exception("Error in RateLimiter for request {request}", ex);
            }
        }
    }
}