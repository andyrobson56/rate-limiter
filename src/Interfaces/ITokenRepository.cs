using System;
using System.Threading.Tasks;
using AirTasker_RateLimiter.Models;

namespace AirTasker_RateLimiter.Interfaces
{
    /// ITokenRepository
    ///
    /// Repository where tokens are stored
    public interface ITokenRepository
    {
        Task<RequestCountToken> GetTokenByIdAsync(string tokenId);
        Task PutToken(string tokenId, RequestCountToken countToken);
    }
}