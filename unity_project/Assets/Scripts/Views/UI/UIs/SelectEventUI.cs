using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

class SelectEventUI : UI {
    protected override string ReferenceName { get; } = "SelectEvent";
    public Image ListSpace { get; private set; }
    readonly float _listspace_max_alpha = 0.392f;

    public Button BackButton { get; private set; }

    public EventButton[] EventButtons { get ; private set; }

    EventDatabase _events_db;

    public void Init(StateMachine state_machine, Image background, UIContainer UI_container, EventDatabase events_db) {
        base.Init(state_machine, background, UI_container);

        _events_db = events_db;

        ListSpace = transform.Find("List Space").GetComponent<Image>();
        
        BackButton = transform.Find("Back Button").GetComponent<Button>();

        int event_n = _events_db.GetEventsList().Count;

        EventButtons = new EventButton[event_n];
        for (int i = 0; i < EventButtons.Length; i++) {
            GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Event Button"), ListSpace.transform.Find("Grid").transform);
            EventButtons[i] = go.GetComponent<EventButton>();
        }

        SetButtonCallbacks();
    }

    void SetButtonCallbacks() {
        BackButton.onClick.AddListener(delegate () {
            _target_UI = _UI_container.MainMenuUI;

            _state_machine.IssueChangeState(new State_MainMenu(_state_machine, _UI_container));
        });

        Dictionary<string, EventSummary> events = _events_db.GetEventsList();
        List<string> keys = events.Keys.ToList();

        for (int i = 0; i < EventButtons.Length; i++)
            EventButtons[i].Init(this, _events_db, keys[i]);
    }

    public void IssueEventLoading(string evt_ID) {
        _state_machine.IssueChangeState(new State_Loading(_state_machine, _UI_container, evt_ID));
    }

    public override IEnumerator Disable() { 
        BackButton.enabled = false;
        foreach (EventButton button in EventButtons)
            button.Disable();

        StartCoroutine(AnimatorFunctions.LinearFade(true, BackButton.targetGraphic, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(true, ListSpace, FADE_TIME, _listspace_max_alpha));
        foreach (EventButton button in EventButtons)
            button.LinearFade(true, FADE_TIME);

        StartCoroutine(BackgroundTransition(true));

        yield return new WaitForSeconds(FADE_TIME);
        yield return new WaitForEndOfFrame();

        gameObject.SetActive(false);
    }

    public override void DisableInstant() {
        BackButton.enabled = false;
        foreach (EventButton button in EventButtons)
            button.Disable();
        gameObject.SetActive(false);
    }

    public override IEnumerator Enable() {
        StartCoroutine(AnimatorFunctions.LinearFade(false, ListSpace, FADE_TIME, _listspace_max_alpha));
        StartCoroutine(AnimatorFunctions.LinearFade(false, BackButton.targetGraphic, FADE_TIME));
        if (_UI_container.Cover.enabled)
            StartCoroutine(AnimatorFunctions.LinearFade(true, _UI_container.Cover, FADE_TIME));
        foreach (EventButton button in EventButtons)
            button.LinearFade(false, FADE_TIME);

        yield return new WaitForSeconds(FADE_TIME);
        yield return new WaitForEndOfFrame();

        _UI_container.Cover.enabled = false;

        BackButton.enabled = true;
        foreach (EventButton button in EventButtons)
            button.Enable();
    }
}
