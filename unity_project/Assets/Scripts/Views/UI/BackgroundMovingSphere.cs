using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class BackgroundMovingSphere : MonoBehaviour {
    const float SPEED = 0.02f;
    const float FADE_TIME = 1;
    
    readonly Vector2 _rot_point = new Vector2(210, 60); //-10, 30 for camera to look up
    Vector3 _axis;
    Image _video_cover;
    public bool Preparing { get; private set; } = false;
    public bool Ready { get; private set; } = false;

    public void Init(Image video_cover) {
        _video_cover = video_cover;
        _axis = SOFConverter.EquirectangularToAbsoluteSphere(_rot_point, transform.lossyScale.x);
    }

    public void PrepareVideo() {
        GetComponent<VideoPlayer>().Prepare();
        Preparing = true;
    }

    // Update is called once per frame
    void Update() {
        if (Preparing && GetComponent<VideoPlayer>().isPrepared) {
            Preparing = false;
            Ready = true;
        }

        if (Ready) {
            if (!GetComponent<VideoPlayer>().isPlaying) {
                StartCoroutine(AnimatorFunctions.LinearFade(true, _video_cover, FADE_TIME));
                GetComponent<VideoPlayer>().Play();
            }

            transform.RotateAround(Vector3.zero, _axis, SPEED);
        }
    }

    void OnDisable() {
        if (_video_cover != null) {
            Preparing = false;
            Ready = false;
            GetComponent<VideoPlayer>().Stop();
            _video_cover.color = new Color(_video_cover.color.r, _video_cover.color.g, _video_cover.color.b, 1);
            _video_cover.gameObject.SetActive(false);
        }
    }

    void OnEnable() {
        if (_video_cover != null) {
            _video_cover.gameObject.SetActive(true);
        }
    }
}
