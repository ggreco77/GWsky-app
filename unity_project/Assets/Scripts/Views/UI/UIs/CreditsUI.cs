using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

class CreditsUI : UI {
    protected override string ReferenceName { get; } = "Credits";

    public Button BackButton { get; private set; }
    public TextMeshProUGUI Title { get; private set; }
    public Image Overlay { get; private set; }

    public new void Init(StateMachine state_machine, Image background, UIContainer UI_container) {
        base.Init(state_machine, background, UI_container);
        
        BackButton = transform.Find("Back Button").GetComponent<Button>();
        Title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        Overlay = transform.Find("Overlay").GetComponent<Image>();

        SetButtonCallbacks();
    }

    void SetButtonCallbacks() {
        BackButton.onClick.AddListener(delegate () {
            _target_UI = _UI_container.MainMenuUI;

            _state_machine.IssueChangeState(new State_MainMenu(_state_machine, _UI_container));
        });
    }

    public override IEnumerator Disable() { 
        BackButton.enabled = false;
        StartCoroutine(AnimatorFunctions.LinearFade(true, Overlay, 1 / 3f * FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(true, BackButton.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(true, Title, FADE_TIME));
        StartCoroutine(BackgroundTransition(true));

        yield return new WaitForSeconds(FADE_TIME);
        yield return new WaitForEndOfFrame();

        gameObject.SetActive(false);
    }

    public override void DisableInstant() {
        BackButton.enabled = false;
        gameObject.SetActive(false);
    }

    public override IEnumerator Enable() {
        StartCoroutine(AnimatorFunctions.LinearFade(false, BackButton.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(false, Title, FADE_TIME));

        yield return new WaitForSeconds(2 / 3f * FADE_TIME);
        StartCoroutine(AnimatorFunctions.LinearFade(false, Overlay, 1 / 3f * FADE_TIME));
        yield return new WaitForSeconds(1 / 3f * FADE_TIME);
        yield return new WaitForEndOfFrame();

        

        BackButton.enabled = true;
    }
}
