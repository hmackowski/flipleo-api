namespace FlipLeo.Core.Exceptions;

/// <summary>Thrown by services when a request breaks a business rule. The API turns this into a 400.</summary>
public class BadRequestException(string message) : Exception(message);
