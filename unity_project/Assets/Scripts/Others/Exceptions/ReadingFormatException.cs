using System;

/// <summary>
/// Exception raised due to error in line reading.
/// </summary>
public class ReadingFormatException : Exception {
    /// <summary>
    /// Constructor.
    /// </summary>
    /// Simply passes the exception message to the base Exception constructor.
    /// <param name="message"></param>
    public ReadingFormatException(string message) : base(message) { }
}
