using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A State Machine with loadable states.
/// </summary>
class StateMachine : MonoBehaviour {
    // List of the states. Used like an inverted stack, with the top at the
    // end of the list, so that traversing the list gives the state in the order they were issued
    readonly List<State> _states = new List<State>();

    // Shorthand for the currently active state
    public State FirstState => (_states.Count > 0) ? _states?[_states.Count - 1] : null;
    // Shorthand for the last state in the stack
    public State LastState => _states?[0];

    /// <summary>
    /// Issue a new state to the StateMachine.
    /// </summary>
    /// <param name="newState">The new state to load</param>
    public void IssueChangeState(State newState) {
        // Execute exiting code for old state, if there is one
        if (_states.Count > 0)
            _states[_states.Count - 1]?.Exit();
        // Execute entering code for new state
        newState.Enter();

        // Remove bottom state in states list, if there is one
        if (_states.Count > 0)
            _states.RemoveAt(0);
        // Add new state to the top of the list
        _states.Add(newState);
    }

    /// <summary>
    /// Issue the StateMachine to enter the previous state.
    /// </summary>
    /// Although unused, the method can prove very helpful to navigate menus.
    public void IssueRevertState() {
        // Execute exiting code for the current state
        FirstState.Exit();
        // Execute entering code for the previous state
        if (_states.Count > 1)
            _states[_states.Count - 2].Enter();
        else
            throw new StateMachineException("Current input state for State Machine is null!\n");
        // Remove current state from state list
        _states.RemoveAt(_states.Count - 1);
    }

    /// <summary>
    /// MonoBehaviour Method. Update is called once per frame.
    /// </summary>
    /// Called once per frame, to perform game logic updating for the current state.
    void Update() {
        // Perform game logic update for the current state
        FirstState?.Update();
    }
}
