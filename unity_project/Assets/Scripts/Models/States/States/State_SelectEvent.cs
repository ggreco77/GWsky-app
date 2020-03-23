/// <summary>
/// Barebone state, does nothing.
/// </summary>
/// Used as a null-valued state, and also as a reference to build all other states.
class State_SelectEvent : State {
    /// <summary>
    /// Constructor.
    /// </summary>
    public State_SelectEvent(StateMachine state_machine, UIContainer UI_container) : base(state_machine) {
        _UI_container = UI_container;
        _select_event_UI = UI_container.SelectEventUI;
    }

    readonly UIContainer _UI_container;
    readonly SelectEventUI _select_event_UI;

    public override void Enter() {
        _UI_container.UISphere.gameObject.SetActive(true);
        _select_event_UI.gameObject.SetActive(true);
        _select_event_UI.StartCoroutine(_select_event_UI.Enable());

        if (!_UI_container.UISphere.Ready && !_UI_container.UISphere.Preparing) {
            _UI_container.UISphere.PrepareVideo();
        }
    }

    public override void Exit() {
        _select_event_UI.StartCoroutine(_select_event_UI.Disable());
    }
}
