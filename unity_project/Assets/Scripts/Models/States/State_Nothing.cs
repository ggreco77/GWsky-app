/// <summary>
/// Barebone state, does nothing.
/// </summary>
/// Used as a null-valued state, and also as a reference to build all other states.
class State_Nothing : State {
    /// <summary>
    /// Constructor.
    /// </summary>
    public State_Nothing() : base(null) { }
}
