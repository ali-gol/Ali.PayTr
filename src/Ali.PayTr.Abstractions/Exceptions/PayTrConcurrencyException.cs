using System;

namespace Ali.PayTr.Abstractions.Exceptions;

/// <summary>
/// Thrown when an optimistic concurrency error occurs during database updates (e.g. duplicate webhook processing).
/// </summary>
public class PayTrConcurrencyException : Exception
{
    public PayTrConcurrencyException(string message) : base(message) { }
    public PayTrConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
}
