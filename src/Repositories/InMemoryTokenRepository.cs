using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AirTasker_RateLimiter.Interfaces;
using AirTasker_RateLimiter.Models;

namespace AirTasker_RateLimiter.Repositories
{
    /// InMemoryTokenRepository
    ///
    /// Basic in memory token store
    public class InMemoryTokenRepository : ITokenRepository
    {
        private ConcurrentDictionary<string, RequestCountToken> _tokens;

        public InMemoryTokenRepository()
        {
            _tokens = new ConcurrentDictionary<string, RequestCountToken>();

        }

        public Task<RequestCountToken> GetTokenByIdAsync(string tokenId)
        {
            RequestCountToken countToken;

            _tokens.TryGetValue(tokenId, out countToken);

            return Task.FromResult(countToken);
        }

        public Task PutToken(string id, RequestCountToken countToken)
        {
            _tokens[id] = countToken;

            return Task.CompletedTask;
        }
    }
}