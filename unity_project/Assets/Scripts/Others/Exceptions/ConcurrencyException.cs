using System;

/// <summary>
/// Exception raised due to generic concurrency issues.
/// </summary>
public class ConcurrencyException : Exception {
    /// <summary>
    /// Constructor.
    /// </summary>
    /// Simply passes the exception message to the base Exception constructor.
    /// <param name="message"></param>
    public ConcurrencyException(string message) : base(message) { }
}
