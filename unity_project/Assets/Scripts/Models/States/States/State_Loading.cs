/// <summary>
/// Barebone state, does nothing.
/// </summary>
/// Used as a null-valued state, and also as a reference to build all other states.
class State_Loading : State {
    /// <summary>
    /// Constructor.
    /// </summary>
    public State_Loading(StateMachine state_machine, UIContainer UI_container, string event_ID) : base(state_machine) {
        _UI_container = UI_container;
        _loading_UI = UI_container.LoadingUI;
        _event_ID = event_ID;
    }

    readonly UIContainer _UI_container;
    readonly LoadingUI _loading_UI;
    readonly string _event_ID;

    public override void Enter() {
        _UI_container.UISphere.gameObject.SetActive(true);
        _loading_UI.gameObject.SetActive(true);
        _loading_UI.SetEventID(_event_ID);
        _loading_UI.StartCoroutine(_loading_UI.Enable());
    }

    public override void Exit() {
        _loading_UI.StartCoroutine(_loading_UI.Disable());
    }
}
