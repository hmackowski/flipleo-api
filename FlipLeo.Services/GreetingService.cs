using FlipLeo.Services.Interfaces;

namespace FlipLeo.Services;

public class GreetingService : IGreetingService
{
    public string GetGreeting() => "Hello from the FlipLeo API22!";
}