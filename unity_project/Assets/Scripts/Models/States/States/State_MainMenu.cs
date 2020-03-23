/// <summary>
/// Barebone state, does nothing.
/// </summary>
/// Used as a null-valued state, and also as a reference to build all other states.
class State_MainMenu : State {
    /// <summary>
    /// Constructor.
    /// </summary>
    public State_MainMenu(StateMachine state_machine, UIContainer UI_container) : base(state_machine) {
        _UI_container = UI_container;
        _main_menu_UI = UI_container.MainMenuUI;
    }

    readonly UIContainer _UI_container;
    readonly MainMenuUI _main_menu_UI;

    public override void Enter() {
        _UI_container.UISphere.gameObject.SetActive(true);
        _main_menu_UI.gameObject.SetActive(true);
        _main_menu_UI.StartCoroutine(_main_menu_UI.Enable());

        if (!_UI_container.UISphere.Ready && !_UI_container.UISphere.Preparing) {
            _UI_container.UISphere.PrepareVideo();
        }
    }

    public override void Exit() {
        _main_menu_UI.StartCoroutine(_main_menu_UI.Disable());
    }
}
