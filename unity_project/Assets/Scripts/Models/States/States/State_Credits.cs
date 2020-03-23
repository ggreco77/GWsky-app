/// <summary>
/// Barebone state, does nothing.
/// </summary>
/// Used as a null-valued state, and also as a reference to build all other states.
class State_Credits : State {
    /// <summary>
    /// Constructor.
    /// </summary>
    public State_Credits(StateMachine state_machine, UIContainer UI_container) : base(state_machine) {
        _UI_container = UI_container;
        _credits_UI = UI_container.CreditsUI;
    }

    readonly UIContainer _UI_container;
    readonly CreditsUI _credits_UI;

    public override void Enter() {
        _UI_container.UISphere.gameObject.SetActive(true);
        _credits_UI.gameObject.SetActive(true);
        _credits_UI.StartCoroutine(_credits_UI.Enable());

        if (!_UI_container.UISphere.Ready && !_UI_container.UISphere.Preparing) {
            _UI_container.UISphere.PrepareVideo();
        }
    }

    public override void Exit() {
        _credits_UI.StartCoroutine(_credits_UI.Disable());
    }
}
