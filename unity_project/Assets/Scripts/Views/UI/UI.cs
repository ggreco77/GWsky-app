using System.Collections;
using UnityEngine;
using UnityEngine.UI;

abstract class UI : MonoBehaviour {
    public const float FADE_TIME = 0.5f;

    protected UIContainer _UI_container;
    protected Image _background;
    protected StateMachine _state_machine;
    protected UI _target_UI;

    protected abstract string ReferenceName { get; }

    protected virtual void Init(StateMachine state_machine, Image background, UIContainer UI_container) {
        _background = background;
        _UI_container = UI_container;
        _state_machine = state_machine;
    }

    protected IEnumerator BackgroundTransition(bool backwards = false) {
        if (_target_UI != null) {
            if (!backwards) {
                string folder = "Images/Background/" + ReferenceName + "TO" + _target_UI.ReferenceName;

                Sprite[] transition_sprites = Resources.LoadAll<Sprite>(folder);

                float wait_between_frames = FADE_TIME / (transition_sprites.Length + 1);
                float i;
                for (i = 0; i < transition_sprites.Length; i += Time.deltaTime / wait_between_frames) {
                    yield return null;
                    _background.sprite = transition_sprites[(int)i];
                }

                for (; i < transition_sprites.Length + 1; i += Time.deltaTime / wait_between_frames)
                    yield return null;
                _background.sprite = Resources.Load<Sprite>("Images/Background/" + _target_UI.ReferenceName + "BG");
            }
            else {
                string folder = "Images/Background/" + _target_UI.ReferenceName + "TO" + ReferenceName;

                Sprite[] transition_sprites = Resources.LoadAll<Sprite>(folder);

                float wait_between_frames = FADE_TIME / (transition_sprites.Length + 1);
                float i;
                for (i = transition_sprites.Length - 1; i > 0; i -= Time.deltaTime / wait_between_frames) {
                    yield return null;
                    _background.sprite = transition_sprites[(int)i];
                }

                for (; i > -1; i -= Time.deltaTime / wait_between_frames)
                    yield return null;
                _background.sprite = Resources.Load<Sprite>("Images/Background/" + _target_UI.ReferenceName + "BG");
            }

            _target_UI = null;
        }
    }

    public abstract IEnumerator Disable();
    public abstract IEnumerator Enable();
    public abstract void DisableInstant();
}
