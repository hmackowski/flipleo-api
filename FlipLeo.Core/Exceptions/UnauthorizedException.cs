namespace FlipLeo.Core.Exceptions;

/// <summary>Bad login or no logged-in user. The API turns this into a 401.</summary>
public class UnauthorizedException(string message) : Exception(message);
