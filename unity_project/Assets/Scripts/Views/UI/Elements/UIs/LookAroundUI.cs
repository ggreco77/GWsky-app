using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

class LookAroundUI : UI {
    protected override string ReferenceName { get; } = "LookAround";

    public CameraRig Camera { get; private set; }
    public SphereAligner SphereAligner { get; private set; }

    public SphereText SphereText { get; private set; }

    public TextMeshProUGUI FirstCalibrationText { get; private set; }

    public Button BackButton { get; private set; }

    public ArrowCamera ArrowCamera { get; private set; }

    public Image DescriptionSpace { get; private set; }

    public Button ChangeTelescopeButton { get; private set; }

    EventDatabase _events_db;
    Transform _photosphere;

    public void Init(StateMachine state_machine, Image background, UIContainer UI_container, CameraRig camera, SphereAligner sphere_aligner, SphereText sphere_text,
                     EventDatabase events_db, Transform photosphere) {
        base.Init(state_machine, background, UI_container);

        Camera = camera;
        SphereAligner = sphere_aligner;
        SphereText = sphere_text;

        _events_db = events_db;
        _photosphere = photosphere;

        FirstCalibrationText = transform.Find("First Calibration Text").GetComponent<TextMeshProUGUI>();
        BackButton = transform.Find("Back Button").GetComponent<Button>();
        ChangeTelescopeButton = transform.Find("Change Telescope Button").GetComponent<Button>();
        ArrowCamera = GameObject.Find("UI Arrow/Arrow Camera").GetComponent<ArrowCamera>();
        DescriptionSpace = transform.Find("Description Container/Scroll Rect/Description Space").GetComponent<Image>();
        
        DescriptionSpace.gameObject.SetActive(false);
        ArrowCamera.Init(camera.GetComponent<Camera>(), events_db, photosphere, this);

        SetButtonCallbacks();
    }

    void SetButtonCallbacks() {
        BackButton.onClick.AddListener(delegate () {
            StartCoroutine(BackToEventSelection());
        });

        ChangeTelescopeButton.onClick.AddListener(delegate () {
            EventData data = _events_db.GetLoadedEvent();
            _photosphere.GetComponent<MeshRenderer>().material.mainTexture = data.Photospheres[(data.CurrentPhotosphere + 1) % data.Photospheres.Length];
            data.CurrentPhotosphere = (data.CurrentPhotosphere + 1) % data.Photospheres.Length;
            ChangeTelescopeButton.transform.Find("Telescope Text").GetComponent<TextMeshProUGUI>().text = data.PhotosphereTypes[data.CurrentPhotosphere];
        });
    }

    public void FirstCalibrationDone() {
        FirstCalibrationText.enabled = false;

        ArrowCamera.Enable();
        StartCoroutine(ArrowCamera.EnableAnimation());
    }

    IEnumerator BackToEventSelection() {
        yield return Disable();
    }

    public IEnumerator DescriptionUnlock() {
        ArrowCamera.CheckCloseness = false;
        yield return ArrowCamera.DisableAnimation();
        ArrowCamera.Disable();
        ArrowCamera.transform.parent.gameObject.SetActive(false);

        // Play wave sound here, based on event
        if (_events_db.GetLoadedEvent().WaveSound != null)
            GetComponent<AudioSource>().Play();

        DescriptionSpace.gameObject.SetActive(true);
        StartCoroutine(AnimatorFunctions.LinearFade(false, DescriptionSpace, FADE_TIME));
        foreach (Transform child in DescriptionSpace.transform)
            StartCoroutine(AnimatorFunctions.LinearFade(false, child.GetComponent<TextMeshProUGUI>(), FADE_TIME));
    }

    void DescriptionSpaceFit() {
        GameObject go_title = DescriptionSpace.transform.Find("Title").gameObject;
        TextMeshProUGUI contents = DescriptionSpace.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        // Set text based on event here
        contents.text = _events_db.GetLoadedEvent().Description;
        contents.ForceMeshUpdate();

        RectTransform title_rect = (RectTransform)go_title.transform;
        float total_height = title_rect.rect.height + 3 * DescriptionSpace.GetComponent<VerticalLayoutGroup>().spacing + contents.preferredHeight;
        DescriptionSpace.rectTransform.sizeDelta = new Vector2(DescriptionSpace.rectTransform.sizeDelta.x, total_height);
    }

    public override IEnumerator Disable() {
        _UI_container.Cover.enabled = true;
        StartCoroutine(AnimatorFunctions.LinearFade(false, _UI_container.Cover, FADE_TIME));
        yield return new WaitForSeconds(FADE_TIME);
        yield return new WaitForEndOfFrame();

        _UI_container.UISphere.gameObject.SetActive(true);
        _UI_container.Background.gameObject.SetActive(true);

        SphereAligner.gameObject.SetActive(false);
        ArrowCamera.transform.parent.gameObject.SetActive(false);
        Camera.transform.eulerAngles = new Vector3(0, 0, 0);
        Camera.enabled = false;
        SphereText.gameObject.SetActive(false);
        DescriptionSpace.rectTransform.anchoredPosition = new Vector2(DescriptionSpace.rectTransform.anchoredPosition.x, 0);
        DescriptionSpace.gameObject.SetActive(false);

        _state_machine.IssueChangeState(new State_SelectEvent(_state_machine, _UI_container));

        gameObject.SetActive(false);
    }

    public override void DisableInstant() {
        SphereAligner.gameObject.SetActive(false);
        ArrowCamera.transform.parent.gameObject.SetActive(false);
        Camera.enabled = false;
        SphereText.gameObject.SetActive(false);
        DescriptionSpace.rectTransform.anchoredPosition = new Vector2(DescriptionSpace.rectTransform.anchoredPosition.x, 0);
        DescriptionSpace.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    public override IEnumerator Enable() {
        SphereAligner.gameObject.SetActive(true);
        ArrowCamera.transform.parent.gameObject.SetActive(true);
        ArrowCamera.Disable();
        Camera.enabled = true;
        SphereText.gameObject.SetActive(true);
        FirstCalibrationText.enabled = true;
        DescriptionSpace.gameObject.SetActive(true);
        DescriptionSpaceFit();
        DescriptionSpace.gameObject.SetActive(false);

        EventData data = _events_db.GetLoadedEvent();
        ChangeTelescopeButton.transform.Find("Telescope Text").GetComponent<TextMeshProUGUI>().text = data.PhotosphereTypes[data.CurrentPhotosphere];
        GetComponent<AudioSource>().clip = data.WaveSound;

        yield return new WaitForSeconds(FADE_TIME);
        yield return new WaitForEndOfFrame();
    }
}
