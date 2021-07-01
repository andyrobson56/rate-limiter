# AirTasker_RateLimiter

# Introduction 
This project is an exercise provided by AirTasker for backend engineers.  The objective is to build a simple rate limiter module that will allow *m* requests every *n* seconds and reject any requests beyond that.

# Design Decisions
## Overview
I chose C# as my language of choice and have implemented the solution is a typical C# project and folder structure.  However, since the test is not about me demonstrating my knowledge and usage of the dotnet framework per se, I have omitted code that one would normally expect to see in a normal dotnet project, e.g. using a DI framework.

## Token Bucket Algorithm
I decided to implement the [Token Bucket](https://en.wikipedia.org/wiki/Token_bucket) algorithm since it is a simple and relatively fast algorithm for rate limiting and since it's being used in telecommunication networks it must be pretty solid.  The disadvantage I see in using this algorithm is you could potentially get a surge of 999 requests in the last second of the hour, then 1000 request in the next second of the new hour, which, depending on the driver for the rate limiter may not be appropriate - if it's a business billing reason then it's fine, but if it's about managing performance then it may not be.

The alternative which I considered was using a rolling window of the last *n* seconds which would counteract the above disadvantage, but this requires much more complexity in storage and retrieval of past request and I felt this would be too slow at runtime for the job at hand.

## Repository
I created the ITokenRepository to abstract away the implementation of the repository, following the Open Close principle of being open to extension.  In real usage, this would be scaled horizontally and an in-memory repository would not achieve the desired result.  For this implementation though, I've settled on an in memory repository.

## Async Await
Essentially any significant code written in dotnet now would be using the async await pattern, and certainly something which would typically sit in the ASP.NET Core middleware should be using this.  So although it is overkill for the in memory repository, I felt the ITokenRepository should use async/await  and if I was to implement a Redis type repository, then it would definitely be required.

## Rate Limiting Strategy
The RateLimiter takes an IRateLimitLevel which wraps the 2 configuration items for the strategy.  This allows for a more generalised solution than is expressed in the requirements.

## Request Abstraction
The very simple IRequest interface hides the complexity of how to determine which requests should be limited together.  In reality this could be based on any number of properties, e.g. requester IP address, API Key, logged in user - all depending on the business requirements.

## Logging and Metrics
I've used serilog as the logging framework, this is to record events that have happened.
I've created an IMetricLogger to demonstrate a simple metric recording interface.  This would be used in a system such as prometheus to record and possibly alert on the nubmer of requests that are being rate limited.

## Future Enhancements
Beyond what is there, I can see some further enhancements that could be implemented without too much trouble.

1. Create an IRateLimitStrategy abstraction to replace the IRateLimitLevels parameters of RateLimiter.  For instance to implement a burst rate strategy.
2. Mutliple rate limiting rules applied to a single request, for instance, 10 requests per second but also 300 requests per minute could be combined.
3. Replace InMemoryTokenRepository with a shared state service such as Redis.
4. Make the rate limiter variables as a configuration item, or even persisted so it could be changed at runtime.
5. Introduce a DI framework.  In practice the dependencies would be injected using the dotnet core DI framework.  This again seemed overkill for the project, but the design follows the D of SOLID so is DI framework ready.

# Getting Started
1.	Installation process
This project requires you to have .NET Core installed which is installed with [the .NET Core SDK](https://www.microsoft.com/net/download).

To build and run the tests, execute the following commands in the root project folder.
```console
dotnet build
dotnet test
```
