using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

class OptionsUI : UI {
    protected override string ReferenceName { get; } = "Options";

    public Button BackButton { get; private set; }

    public TextMeshProUGUI Title { get; private set; }

    public CheckBox DebugCheckbox { get; private set; }

    public new void Init(StateMachine state_machine, Image background, UIContainer UI_container) {
        base.Init(state_machine, background, UI_container);
        
        BackButton = transform.Find("Back Button").GetComponent<Button>();
        Title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        DebugCheckbox = transform.Find("Debug Checkbox").GetComponent<CheckBox>();
        DebugCheckbox.Init(Options.DebugPrint);

        SetButtonCallbacks();
    }

    void SetButtonCallbacks() {
        BackButton.onClick.AddListener(delegate () {
            _target_UI = _UI_container.MainMenuUI;

            _state_machine.IssueChangeState(new State_MainMenu(_state_machine, _UI_container));
        });

        DebugCheckbox.onClick.AddListener(delegate () {
            Options.DebugPrint = DebugCheckbox.Toggle();
        });
    }

    public override IEnumerator Disable() { 
        BackButton.enabled = false;
        DebugCheckbox.enabled = false;

        StartCoroutine(AnimatorFunctions.LinearFade(true, BackButton.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(true, DebugCheckbox.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(true, DebugCheckbox.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(true, Title, FADE_TIME));
        StartCoroutine(BackgroundTransition(true));

        yield return new WaitForSeconds(FADE_TIME);
        yield return new WaitForEndOfFrame();

        gameObject.SetActive(false);
    }

    public override void DisableInstant() {
        BackButton.enabled = false;
        DebugCheckbox.enabled = false;
        gameObject.SetActive(false);
    }

    public override IEnumerator Enable() {
        StartCoroutine(AnimatorFunctions.LinearFade(false, BackButton.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(false, DebugCheckbox.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(false, DebugCheckbox.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(false, Title, FADE_TIME));

        yield return new WaitForSeconds(FADE_TIME);
        yield return new WaitForEndOfFrame();

        DebugCheckbox.enabled = true;
        BackButton.enabled = true;
    }
}
