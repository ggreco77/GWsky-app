using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class BackgroundMovingSphere : MonoBehaviour {
    const float SPEED = 0.02f;
    const float FADE_TIME = 1;
    
    const int MAX_WAIT_FOR_VIDEO = 180; // In frames
    int _curr_wait_for_video = 0;

    readonly Vector2 _rot_point = new Vector2(210, 60); //-10, 30 for camera to look up
    Vector3 _axis;
    Image _video_cover;
    public bool Preparing { get; private set; } = false;
    public bool Ready { get; private set; } = false;
    public bool Skip { get; private set; } = false;
    public bool SkipReady { get; private set; } = true;

    public void Init(Image video_cover) {
        _video_cover = video_cover;
        _axis = SOFConverter.EquirectangularToAbsoluteSphere(_rot_point, transform.lossyScale.x);
        GetComponent<VideoPlayer>().errorReceived += OnError;
        _curr_wait_for_video = 0;
    }

    void OnError(VideoPlayer source, string message) {
        Skip = true;
        SkipReady = true;
        Log.Print("Video Player error! " + message, Log.Colors.Error);
    }

    public void PrepareVideo() {
        Preparing = true;
        GetComponent<VideoPlayer>().Prepare();
    }

    // Update is called once per frame
    void Update() {
        if (!Skip) {
            if (Preparing && GetComponent<VideoPlayer>().isPrepared) {
                Preparing = false;
                Ready = true;
            }

            if (Preparing && !GetComponent<VideoPlayer>().isPrepared) {
                _curr_wait_for_video++;
                if (_curr_wait_for_video >= MAX_WAIT_FOR_VIDEO) {
                    OnError(GetComponent<VideoPlayer>(), "Attempt to read video file has timeout!");
                    _curr_wait_for_video = 0;
                }
            }

            if (Ready && !Skip) {
                if (!GetComponent<VideoPlayer>().isPlaying) {
                    StartCoroutine(AnimatorFunctions.LinearFade(true, _video_cover, FADE_TIME));
                    GetComponent<VideoPlayer>().Play();
                }
            }
        }
        else {
            if (SkipReady) {
                GetComponent<VideoPlayer>().enabled = false;
                StartCoroutine(AnimatorFunctions.LinearFade(true, _video_cover, FADE_TIME));
                Preparing = false;
                SkipReady = false;
                Ready = false;
            }
        }

        transform.RotateAround(Vector3.zero, _axis, SPEED);
    }

    void OnDisable() {
            if (_video_cover != null) {
                Preparing = false;
                Ready = false;
                if (GetComponent<VideoPlayer>().enabled)
                    GetComponent<VideoPlayer>().Stop();
                _video_cover.color = new Color(_video_cover.color.r, _video_cover.color.g, _video_cover.color.b, 1);
                _video_cover.gameObject.SetActive(false);
            SkipReady = true;
            }
    }

    void OnEnable() {
        if (_video_cover != null) {
            _video_cover.gameObject.SetActive(true);
        }
    }
}
