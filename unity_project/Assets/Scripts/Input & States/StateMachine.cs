using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls inputs through the InputController and based on the current state.

public class StateMachine : MonoBehaviour {

    Master _master;
    SphereAligner _sphere_aligner;

    List<State> _states = new List<State>();
    const int _states_limit = 20;
    const int SPHERE_ALIGN_CD = 90;
    int sphere_align_count = SPHERE_ALIGN_CD - 1;

    State[] states;

    public void Init(Master master, SphereAligner sphere_aligner)
    {
        _master = master;
        _sphere_aligner = sphere_aligner;

        // Fill states list with empty states
        for (int i = 0; i < _states_limit; i++)
        {
            State_Nothing _state = new State_Nothing(_master);
            _states.Add(_state);
        }
    }

    //Change this function to work with DebugMessages
    /*Print the current list of states, useful for debugging
    public void PrintStates()
    {
        string debug_str = "";
        foreach(State state in _states)
        {
            debug_str += state.GetType() + " ";
        }
        Debug.Log(debug_str);
    }
    */

    public void IssueChangeState(State newState)
    {
        _states[_states_limit - 1].Exit();
        newState.Enter();

        // Remove bottom state in stack
        _states.RemoveAt(0);
        // Add new state to top
        _states.Add(newState);
    }
    public void IssueRevertState()
    {
        _states[_states_limit - 1].Exit();
        _states[_states_limit - 2].Enter();

        // Remove bottom state in stack
        _states.RemoveAt(_states_limit - 1);
        // Add new state to top
        State_Nothing state = new State_Nothing(_master);
        _states.Insert(0, state);

        if (_states[_states_limit - 1] is State_Nothing)
        {
            DebugMessages.Print("Logic error: Current input state for State Machine is zeroed state!\n", DebugMessages.Colors.Error);
        }
    }

    // Update is called once per frame
    void Update() {
        _master.main_camera.Rotate();

        sphere_align_count++;
        if (sphere_align_count == SPHERE_ALIGN_CD)
        {
            Input.compass.enabled = true;
            Input.location.Start();
            sphere_align_count = 0;
            _sphere_aligner.AlignSphere();
            DebugMessages.Print("Local Coordinates of point of RA_Dec (180, 0): " + _sphere_aligner.az_h.ToString());
            DebugMessages.PrintClear("Magnetometer: " + Input.compass.rawVector.normalized.ToString());
            DebugMessages.Print("Accelerometer: " + Input.gyro.gravity.normalized.ToString());
            DebugMessages.Print("Compensated North Heading: " + CompassHandler.CompensatedHeading(Input.compass.rawVector,
                                                                       new Vector3(Input.gyro.gravity.x, Input.gyro.gravity.z, Input.gyro.gravity.y)));
            DebugMessages.Print("GPS: " + _sphere_aligner.GPS);
            float phone_dir = (float)MathExtension.ToDegrees(System.Math.Atan2(Input.gyro.attitude.y, Input.gyro.attitude.x));
            DebugMessages.Print("Phone Heading: " + phone_dir.ToString());
            Input.location.Stop();
        }

        _states[_states_limit - 1].Update();
    }
}

public abstract class State
{
    public Master master;
    public StateMachine sm;

    public State(Master master)
    {
        this.master = master;
        sm = master.state_machine;
    }

    public virtual void Update()
    {
    }

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }
}