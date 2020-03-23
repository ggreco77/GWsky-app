

/// <summary>
/// A state which can be fed to a StateMachine via method IssueStateChange().
/// Contains overridable methods which are executed when entering or exiting the state machine,
/// as well as a method which is executed each frame the state is active within the machine.
/// </summary>
abstract class State {
    // Reference to the state machine used by this entity
    public StateMachine StateMachine { get; private set; }

    /// <summary>
    /// Constructor. Passes reference to Master singleton.
    /// </summary>
    /// <param name="master"></param>
    public State(StateMachine state_machine) {
        // Pass parameters to local variables
        StateMachine = state_machine;
    }

    /// <summary>
    /// Overridable method which is executed each frame the state is at the top of the StateMachine state stack.
    /// </summary>
    public virtual void Update() {
    }

    /// <summary>
    /// Overridable method which is executed when the state enters the StateMachine.
    /// </summary>
    public virtual void Enter() {
    }

    /// <summary>
    /// Overridable method which is executed when the state exits the StateMachine.
    /// </summary>
    public virtual void Exit() {
    }
}