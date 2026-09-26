namespace FlipLeo.Core.Exceptions;

/// <summary>The request clashes with existing data (e.g. email already registered). The API turns this into a 409.</summary>
public class ConflictException(string message) : Exception(message);
