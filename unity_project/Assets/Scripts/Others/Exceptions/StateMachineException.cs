using System;

/// <summary>
/// Exception raised due to error in the state machine's current state.
/// </summary>
public class StateMachineException : Exception {
    /// <summary>
    /// Constructor.
    /// </summary>
    /// Simply passes the exception message to the base Exception constructor.
    /// <param name="message"></param>
    public StateMachineException(string message) : base(message) { }
}
