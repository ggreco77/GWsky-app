/// <summary>
/// Barebone state, does nothing.
/// </summary>
/// Used as a null-valued state, and also as a reference to build all other states.
class State_LookAround : State {
    /// <summary>
    /// Constructor.
    /// </summary>
    public State_LookAround(StateMachine state_machine, UIContainer UI_container, SphereAligner sphere_aligner) : base(state_machine) {
        _UI_container = UI_container;
        _lookaround_UI = UI_container.LookAroundUI;
        _sphere_aligner = sphere_aligner;
    }

    readonly UIContainer _UI_container;
    readonly LookAroundUI _lookaround_UI;

    readonly SphereAligner _sphere_aligner;

    const int SPHERE_ALIGN_CD = 1800;  // In frames
    int _curr_sphere_align_cd = SPHERE_ALIGN_CD - 1;

    public override void Enter() {
        _lookaround_UI.SphereAligner.gameObject.SetActive(true);
        _lookaround_UI.Camera.enabled = true;
        _lookaround_UI.SphereText.gameObject.SetActive(true);

        _lookaround_UI.gameObject.SetActive(true);
        _lookaround_UI.StartCoroutine(_lookaround_UI.Enable());
    }

    public override void Update() {
        _curr_sphere_align_cd++;
        if (_curr_sphere_align_cd == SPHERE_ALIGN_CD) {
            _curr_sphere_align_cd = 0;
            if (_sphere_aligner != null)
            _sphere_aligner.AlignSphere();
        }
    }
}
