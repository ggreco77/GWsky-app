using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

class MainMenuUI : UI {
    public TextMeshProUGUI Title { get; private set; }

    public Button SelectEventButton { get; private set; }
    public Button OptionsButton { get; private set; }
    public Button CreditsButton { get; private set; }
    public Button ExitButton { get; private set; }

    bool _first_time = true;

    protected override string ReferenceName { get; } = "MainMenu";

    public new void Init(StateMachine state_machine, Image background, UIContainer UI_container) {
        base.Init(state_machine, background, UI_container);
        Title = transform.Find("Title").GetComponent<TextMeshProUGUI>();

        SelectEventButton = transform.Find("Select Event Button").GetComponent<Button>();
        OptionsButton = transform.Find("Options Button").GetComponent<Button>();
        CreditsButton = transform.Find("Credits Button").GetComponent<Button>();
        ExitButton = transform.Find("Exit Button").GetComponent<Button>();
        
        float equidistance = Screen.width / 30;

        SelectEventButton.transform.position = new Vector3(6 * equidistance, SelectEventButton.transform.position.y, SelectEventButton.transform.position.z);
        OptionsButton.transform.position = new Vector3(12 * equidistance, OptionsButton.transform.position.y, OptionsButton.transform.position.z);
        CreditsButton.transform.position = new Vector3(18 * equidistance, CreditsButton.transform.position.y, CreditsButton.transform.position.z);
        ExitButton.transform.position = new Vector3(24 * equidistance, ExitButton.transform.position.y, ExitButton.transform.position.z);

        SetButtonCallbacks();
    }

    void SetButtonCallbacks() {
        ExitButton.onClick.AddListener(delegate () {
            _UI_container.Cover.enabled = true;
            StartCoroutine(Quit());
        });

        SelectEventButton.onClick.AddListener(delegate () {
            _target_UI = _UI_container.SelectEventUI;

            _state_machine.IssueChangeState(new State_SelectEvent(_state_machine, _UI_container));
        });

        OptionsButton.onClick.AddListener(delegate () {
            _target_UI = _UI_container.OptionsUI;

            _state_machine.IssueChangeState(new State_Options(_state_machine, _UI_container));
        });

        CreditsButton.onClick.AddListener(delegate () {
            _target_UI = _UI_container.CreditsUI;

            _state_machine.IssueChangeState(new State_Credits(_state_machine, _UI_container));
        });
    }

    IEnumerator Quit() {
        StartCoroutine(AnimatorFunctions.LinearFade(false, _UI_container.Cover, FADE_TIME));

        yield return new WaitForSeconds(FADE_TIME);
        yield return new WaitForEndOfFrame();

        Application.Quit();
    }

    public override IEnumerator Disable() {
        SelectEventButton.enabled = false;
        OptionsButton.enabled = false;
        CreditsButton.enabled = false;
        ExitButton.enabled = false;

        StartCoroutine(AnimatorFunctions.LinearFade(true, Title, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(true, SelectEventButton.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(true, OptionsButton.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(true, CreditsButton.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(true, ExitButton.targetGraphic, FADE_TIME));
        StartCoroutine(BackgroundTransition());

        yield return new WaitForSeconds(FADE_TIME);
        yield return new WaitForEndOfFrame();

        gameObject.SetActive(false);
    }

    public override void DisableInstant() {
        SelectEventButton.enabled = false;
        OptionsButton.enabled = false;
        CreditsButton.enabled = false;
        ExitButton.enabled = false;

        gameObject.SetActive(false);
    }

    public override IEnumerator Enable() {
        StartCoroutine(AnimatorFunctions.LinearFade(false, Title, FADE_TIME));
        if (_first_time) {
            StartCoroutine(AnimatorFunctions.LinearFade(false, _background, FADE_TIME));
            _first_time = false;
        }
        StartCoroutine(AnimatorFunctions.LinearFade(false, SelectEventButton.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(false, OptionsButton.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(false, CreditsButton.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(false, ExitButton.targetGraphic, FADE_TIME));

        yield return new WaitForSeconds(FADE_TIME);
        yield return new WaitForEndOfFrame();

        SelectEventButton.enabled = true;
        OptionsButton.enabled = true;
        CreditsButton.enabled = true;
        ExitButton.enabled = true;
    }
}
