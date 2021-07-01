# AirTasker_RateLimiter

# Introduction 
This project is an exercise provided by AirTasker for backend engineers.  The objective is to build a simple rate limiter module that will allow m requests every n seconds and reject any requests beyond that.

# Design Decisions
## Token Bucket Algorithm
I decided to implement the [Token Bucket](https://en.wikipedia.org/wiki/Token_bucket) algorithm since it is a simple and relatively fast algorithm for rate limiting and if its being used in telecommunication networks it must be pretty solid.  The disadvantage using this algorithm is you could potentially get a surge of 999 requests in the last second of the hour, then 1000 request in the next second of the new hour, which, depending on the driver for the rate limiter may not be appropriate - if its business billing reason then its fine, but if its about managing performance then it may not be.
The alternative which I considered was using a rolling window of the last n seconds which would counteract the above disadvantage, but this requires much more complexity in storage and retrieval of past request and I felt this would be too slow at runtime for the job at hand.  

## Repository
I abstracted the token repository as I felt in real usage request handlers could well be horizontally scaled, and a simple in memory rate limiter would not achieve the desired result.  Instead, the repository be best served by a Redis cache that could be share amongst all instances of the endpoint.  For this implementation though, I've settled on an in memory repository.

## Async Await
Essentially any significant code written in dotnet now would be using the async await pattern, and certainly something which would typically sit in the ASP.NET Core middleware should be using this.  So although for the in memory repository it is overkill and I've had to manafacture await'able methods, I felt the ITokenRepository should be ready and if I was to implement a Redis type repository, then it would definitely be required.

## Rate Limiting Strategy
The RateLimiter as is, takes the 2 variables that define the request and time limits.  This allows a more generalised approach than that defined in the requirements.  

## Request Abstraction
The very simple IRequest interface hides the complexity of how to determine which requests should be limited together.  In reality this could be by requester IP address or API Key or logged in user - all depending on the business requirements.

## Future Enhancements
Beyond what is there, I can see some further enhancements that could be implemented without too much trouble.
1. IRateLimitStrategy to replace the request and seconds parameters of RateLimiter.  This would allow more complex or different ways of determining if any given request is valid now.
2. Mutliple rate limiting rules applied to a single request, for instance, 10 requests per second but also 300 requests per minute could be combined.
3. Replace InMemoryTokenRepository with a shared state service such as Redis
4. Introduce a DI framework.  In practice the dependencies would be injected using the dotnet core DI framework.  This again seemed overkill for the project, but the design follows the D of SOLID so is DI framework ready.

# Getting Started
1.	Installation process
This project requires you to have .NET Core installed which is installed with [the .NET Core SDK](https://www.microsoft.com/net/download).

To build and run the tests, execute the following commands in the root project folder.
```console
dotnet build
dotnet test
```
