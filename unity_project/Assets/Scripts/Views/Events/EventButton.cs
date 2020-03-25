using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

class EventButton : Button {
    public const float MAX_ALPHA = 0.85f;
    Color _pressed_color = new Color(0.7843f, 0.7843f, 0.7843f, 1);
    Color _normal_color = new Color(1, 1, 1, 1);
    Color _disabled_color = new Color(0.5f, 0.5f, 0.5f, 1);

    Color _normal_partial_color = new Color(1, 1, 1, MAX_ALPHA);
    Color _disabled_partial_color = new Color(0.5f, 0.5f, 0.5f, MAX_ALPHA);

    Button _play_button;
    Button _delete_button;
    Button _download_button;

    TextMeshProUGUI _name;
    TextMeshProUGUI _size;
    TextMeshProUGUI _date;

    EventDatabase _events_db;
    string _ID;
    SelectEventUI _select_event_UI;

    /* Fits the element width to the grid width, so that the element fills the canvas horizontally.
     * Function must be called once due to Unity using rt.sizeDelta to fit elements, which in this case
     * would produce this element to have width 0 because the grid is also dynamically fitted. */
    void PlaceElements() {
        /* Get the element and parent RectTransforms. */
        RectTransform parent_rt = (RectTransform)transform.parent;

        RectTransform rt = (RectTransform)transform;
        /* Fit horizontally using the parent width, do not touch height. */
        rt.sizeDelta = new Vector2(parent_rt.rect.width, rt.sizeDelta.y);

        RectTransform title_rt = (RectTransform)_name.transform;
        title_rt.sizeDelta = new Vector2(parent_rt.rect.width, rt.sizeDelta.y);
        title_rt.position = new Vector3(Screen.width / 5 + Screen.width / 15,
                                        title_rt.position.y,
                                        title_rt.position.z);

        RectTransform date_rt = (RectTransform)_date.transform;
        date_rt.sizeDelta = new Vector2(parent_rt.rect.width, rt.sizeDelta.y);
        date_rt.position = new Vector3(Screen.width / 2 + Screen.width / 7,
                                       date_rt.position.y,
                                       date_rt.position.z);

        RectTransform size_rt = (RectTransform)_size.transform;
        size_rt.sizeDelta = new Vector2(parent_rt.rect.width, rt.sizeDelta.y);
        size_rt.position = new Vector3(Screen.width / 2 + Screen.width / 7,
                                       size_rt.position.y,
                                       size_rt.position.z);

        _download_button.transform.position = new Vector3(-Screen.width / 2 + Screen.width / 12,
                                                          _download_button.transform.position.y,
                                                          _download_button.transform.position.z);

        _play_button.transform.position = new Vector3(-Screen.width / 2 + Screen.width / 12,
                                                      _play_button.transform.position.y,
                                                      _play_button.transform.position.z);

        _delete_button.transform.position = new Vector3(-Screen.width / 2 + Screen.width / 6,
                                                        _delete_button.transform.position.y,
                                                        _delete_button.transform.position.z);
    }

    public void LinearFade(bool fade_out, float time) {
        StartCoroutine(AnimatorFunctions.LinearFade(fade_out, _play_button.targetGraphic, time));
        StartCoroutine(AnimatorFunctions.LinearFade(fade_out, _delete_button.targetGraphic, time));
        StartCoroutine(AnimatorFunctions.LinearFade(fade_out, _download_button.targetGraphic, time));

        StartCoroutine(AnimatorFunctions.LinearFade(fade_out, targetGraphic, time, MAX_ALPHA));

        StartCoroutine(AnimatorFunctions.LinearFade(fade_out, _date, time));
        StartCoroutine(AnimatorFunctions.LinearFade(fade_out, _name, time));
        StartCoroutine(AnimatorFunctions.LinearFade(fade_out, _size, time));
    }

    void ChangeEventAvailability(bool local) {
        if (local) {
            _download_button.gameObject.SetActive(false);
            _play_button.gameObject.SetActive(true);
            _delete_button.gameObject.SetActive(true);

            GetComponent<Image>().color = _normal_partial_color;
            colors = new ColorBlock() {
                normalColor = _normal_color,
                highlightedColor = _normal_color,
                pressedColor = _pressed_color,
                selectedColor = _normal_color,
                disabledColor = _normal_color,
                colorMultiplier = 1,
                fadeDuration = 0.1f
            };
        }
        else {
            _download_button.gameObject.SetActive(true);
            _play_button.gameObject.SetActive(false);
            _delete_button.gameObject.SetActive(false);

            GetComponent<Image>().color = _disabled_partial_color;
            colors = new ColorBlock() {
                normalColor = _normal_color,
                highlightedColor = _normal_color,
                pressedColor = _normal_color,
                selectedColor = _normal_color,
                disabledColor = _normal_color,
                colorMultiplier = 1,
                fadeDuration = 0.1f
            };
        }
    }

    void SetButtonCallbacks() {
        _play_button.onClick.AddListener(delegate() {
            _select_event_UI.IssueEventLoading(_ID);
        });
        
        _download_button.onClick.AddListener(delegate() {
            _events_db.DownloadEvent(_ID);

            ChangeEventAvailability(_events_db.GetEventsList()[_ID].OnLocalStorage);
        });
        
        _delete_button.onClick.AddListener(delegate() {
            _events_db.DeleteEvent(_ID);

            ChangeEventAvailability(_events_db.GetEventsList()[_ID].OnLocalStorage);
        });
    }

    public void Init(SelectEventUI select_event_UI, EventDatabase events_db, string ID) {
        _ID = ID;
        _events_db = events_db;
        _select_event_UI = select_event_UI;

        Dictionary<string, EventSummary> events = events_db.GetEventsList();

        _play_button = transform.transform.Find("Play Button").GetComponent<Button>();
        _delete_button = transform.transform.Find("Delete Button").GetComponent<Button>();
        _download_button = transform.transform.Find("Download Button").GetComponent<Button>();

        _name = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        _date = transform.Find("Date").GetComponent<TextMeshProUGUI>();
        _size = transform.Find("Size").GetComponent<TextMeshProUGUI>();

        _name.text = events[ID].Name;
        _date.text = "Date: " + events[ID].Date + " UTC";
        _size.text = events[ID].Size + " Mb";

        SetButtonCallbacks();
        ChangeEventAvailability(events[ID].OnLocalStorage);

        PlaceElements();
    }

    public void Enable() {
        enabled = true;

        _play_button.enabled = true;
        _download_button.enabled = true;
        _delete_button.enabled = true;
    }

    public void Disable() {
        enabled = false;

        _play_button.enabled = false;
        _download_button.enabled = false;
        _delete_button.enabled = false;
    }
}
