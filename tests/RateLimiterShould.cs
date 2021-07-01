using System;
using Xunit;
using Moq;
using AirTasker_RateLimiter.Interfaces;
using AirTasker_RateLimiter.Services;
using Serilog;
using AirTasker_RateLimiter.Models;

namespace AirTasker_RateLimiter.UnitTests
{
    public class RateLimiterShould
    {
        private static int HOUR = 60*60;

        private IRateLimiter _rateLimiter;
        private ITokenRepository _tokenRepository;
        private readonly ILogger _logger;
        private readonly IMetricLogger _metricLogger;
        private readonly IRequest _request;

        public RateLimiterShould()
        {
            var requestMock = new Mock<IRequest>();
            requestMock.Setup(request => request.Id()).Returns("tokenId");
            _request = requestMock.Object;

            // Just mock out the loggers
            _logger = new Mock<ILogger>().Object;
            _metricLogger = new Mock<IMetricLogger>().Object;
        }

        protected void Setup(RequestCountToken token, RateLimitLevels rateLimitLevels)
        {
            var mock = new Mock<ITokenRepository>();

            mock.Setup(repo => repo.GetTokenByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(() => token);

            _tokenRepository = mock.Object;

            _rateLimiter = new RateLimiter(rateLimitLevels, _tokenRepository, _logger, _metricLogger);
        }


        [Fact]
        public async void RateLimiter_ShouldAcceptFirst()
        {
            var rateLimitLevels = new RateLimitLevels(seconds: HOUR, requests: 1);
            RequestCountToken token = null;

            Setup(token, rateLimitLevels);

            var result = await _rateLimiter.IsRequestRateLimitedAsync(_request);

            Assert.Equal((200, string.Empty), result);
        }

        [Fact]
        public async void RateLimiter_ShouldRejectWhenLimitReached()
        {
            var rateLimitLevels = new RateLimitLevels(seconds: 100, requests: 100);
            var token = new RequestCountToken(startTime: DateTime.UtcNow, requestCount: 100);

            Setup(token, rateLimitLevels);

            var result = await _rateLimiter.IsRequestRateLimitedAsync(_request);

            Assert.Equal(429, result.ReturnCode);
        }

        [Fact]
        public async void RateLimiter_ShouldReturnErrorMessageOnReject()
        {
            var rateLimitLevels = new RateLimitLevels(seconds: 100, requests: 100);
            var token = new RequestCountToken(startTime: DateTime.UtcNow, requestCount: 150);

            Setup(token, rateLimitLevels);

            var result = await _rateLimiter.IsRequestRateLimitedAsync(_request);

            Assert.Matches("Rate limit exceeded. Try again in #[0-9]* seconds", result.Message);
        }
        

        [Fact]
        public async void RateLimiter_ShouldAcceptWithExpiredToken()
        {
            var rateLimitLevels = new RateLimitLevels(seconds: 100, requests: 100);

            var expiredTokenStartTime = DateTime.UtcNow.AddSeconds(-110);
            var expiredToken = new RequestCountToken(startTime: expiredTokenStartTime, requestCount: 150);

            Setup(expiredToken, rateLimitLevels);

            var result = await _rateLimiter.IsRequestRateLimitedAsync(_request);

            Assert.Equal((200, string.Empty), result);
        }
    }
}