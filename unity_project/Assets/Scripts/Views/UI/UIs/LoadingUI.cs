using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

class LoadingUI : UI {
    protected override string ReferenceName { get; } = "Loading";

    public TextMeshProUGUI LoadingText {get; private set; }

    Transform _photosphere;
    EventDatabase _events_db;
    string _event_ID;
    SphereAligner _sphere_aligner;

    public void Init(StateMachine state_machine, Image background, UIContainer UI_container, Transform photosphere, EventDatabase events_db, SphereAligner sphere_aligner) {
        base.Init(state_machine, background, UI_container);

        LoadingText = transform.Find("Loading...").GetComponent<TextMeshProUGUI>();

        _photosphere = photosphere;
        _events_db = events_db;
        _sphere_aligner = sphere_aligner;
    }

    public void SetEventID(string ID) {
        _event_ID = ID;
    }

    public override IEnumerator Disable() { 
        StartCoroutine(AnimatorFunctions.LinearFade(true, LoadingText, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(true, _UI_container.Cover, FADE_TIME));

        yield return new WaitForSeconds(FADE_TIME);
        yield return new WaitForEndOfFrame();

        gameObject.SetActive(false);
        _UI_container.Cover.enabled = false;
    }

    public override void DisableInstant() {
        gameObject.SetActive(false);
        _UI_container.Cover.enabled = false;
    }

    public override IEnumerator Enable() {
        _UI_container.Cover.enabled = true;

        StartCoroutine(AnimatorFunctions.LinearFade(false, LoadingText, FADE_TIME));
        StartCoroutine(AnimatorFunctions.LinearFade(false, _UI_container.Cover, FADE_TIME));

        yield return new WaitForSeconds(FADE_TIME);
        yield return new WaitForEndOfFrame();

        _UI_container.UISphere.gameObject.SetActive(false);
        _UI_container.Background.gameObject.SetActive(false);

        StartCoroutine(ApplyEventPhotosphere());
    }

    IEnumerator ApplyEventPhotosphere() {
        yield return _events_db.LoadEvent(_event_ID, _photosphere);
        EventData data = _events_db.GetLoadedEvent();

        _photosphere.GetComponent<MeshRenderer>().material.mainTexture = data.Photospheres[0];
        data.CurrentPhotosphere = 0;

        yield return new WaitForSeconds(0.1f);

        _state_machine.IssueChangeState(new State_LookAround(_state_machine, _UI_container,  _sphere_aligner));
    }
}
