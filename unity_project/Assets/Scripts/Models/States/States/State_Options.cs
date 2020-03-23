/// <summary>
/// Barebone state, does nothing.
/// </summary>
/// Used as a null-valued state, and also as a reference to build all other states.
class State_Options : State {
    /// <summary>
    /// Constructor.
    /// </summary>
    public State_Options(StateMachine state_machine, UIContainer UI_container) : base(state_machine) {
        _UI_container = UI_container;
        _options_UI = UI_container.OptionsUI;
    }

    readonly UIContainer _UI_container;
    readonly OptionsUI _options_UI;

    public override void Enter() {
        _UI_container.UISphere.gameObject.SetActive(true);
        _options_UI.gameObject.SetActive(true);
        _options_UI.StartCoroutine(_options_UI.Enable());

        if (!_UI_container.UISphere.Ready && !_UI_container.UISphere.Preparing && !_UI_container.UISphere.Skip) {
            _UI_container.UISphere.PrepareVideo();
        }
    }

    public override void Exit() {
        _options_UI.StartCoroutine(_options_UI.Disable());
    }
}
