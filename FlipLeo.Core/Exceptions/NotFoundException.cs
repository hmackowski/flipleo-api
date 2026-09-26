namespace FlipLeo.Core.Exceptions;

/// <summary>Thrown by services when a requested record doesn't exist. The API turns this into a 404.</summary>
public class NotFoundException(string message) : Exception(message);
