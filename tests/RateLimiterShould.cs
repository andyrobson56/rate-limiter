using System;
using Xunit;
using Moq;
using AirTasker_RateLimiter.Interfaces;
using AirTasker_RateLimiter.Repositories;
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

        protected void Setup(RequestCountToken token, int requests, int seconds)
        {
            var mock = new Mock<ITokenRepository>();

            mock.Setup(repo => repo.GetTokenByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(() => token);

            _tokenRepository = mock.Object;

            _rateLimiter = new RateLimiter(requests, seconds, _tokenRepository, _logger, _metricLogger);
        }


        [Fact]
        public async void RateLimiter_ShouldAcceptFirst()
        {
            int allowedRequests = 1;
            int allowedSeconds = HOUR;
            RequestCountToken token = null;

            Setup(token, allowedRequests, allowedSeconds);

            var result = await _rateLimiter.IsRequestRateLimitedAsync(_request);

            Assert.Equal((200, string.Empty), result);
        }

        [Fact]
        public async void RateLimiter_ShouldRejectWhenLimitReached()
        {
            int allowedRequests = 100;
            int allowedSeconds = 100;

            var tokenStartTime = DateTime.UtcNow;
            var requestsReceived = 100;

            var expiredToken = new RequestCountToken(tokenStartTime, requestsReceived);

            Setup(expiredToken, allowedRequests, allowedSeconds);

            var result = await _rateLimiter.IsRequestRateLimitedAsync(_request);

            Assert.Equal(429, result.ReturnCode);
        }

        [Fact]
        public async void RateLimiter_ShouldReturnErrorMessageOnReject()
        {
            int allowedRequests = 100;
            int allowedSeconds = 100;

            var tokenStartTime = DateTime.UtcNow;
            var requestsReceived = 150;

            var expiredToken = new RequestCountToken(tokenStartTime, requestsReceived);

            Setup(expiredToken, allowedRequests, allowedSeconds);

            var result = await _rateLimiter.IsRequestRateLimitedAsync(_request);

            Assert.Matches("Rate limit exceeded. Try again in #[0-9]* seconds", result.Message);
        }
        

        [Fact]
        public async void RateLimiter_ShouldAcceptWithExpiredToken()
        {
            int allowedRequests = 100;
            int allowedSeconds = 100;

            var tokenStartTime = DateTime.UtcNow.AddSeconds(-allowedSeconds - 10);
            var requestsReceived = 150;

            var expiredToken = new RequestCountToken(tokenStartTime, requestsReceived);

            Setup(expiredToken, allowedRequests, allowedSeconds);

            var result = await _rateLimiter.IsRequestRateLimitedAsync(_request);

            Assert.Equal((200, string.Empty), result);
        }
    }
}