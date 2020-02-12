using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System;

public class SphereText : MonoBehaviour
{
    const float TEXTSPHERE_RADIUS = 9.0f;

    Dictionary<string, Text> _texts = new Dictionary<string, Text>();
    Camera _camera;

    //Base path containing all events data.
    string starsfile_path = "";

    public void Init(Camera main_camera)
    {
        _camera = main_camera;

        starsfile_path = Application.persistentDataPath + "/Stars/stars.txt";

        Directory.CreateDirectory(Application.persistentDataPath + "/Stars/");
        if (!File.Exists(starsfile_path)) {
            TextAsset stars_data = Resources.Load("Packets/Stars/Stars") as TextAsset;
            File.WriteAllBytes(starsfile_path, stars_data.bytes);
        }
    }

    public void LoadFromTextFile(Transform sphere) {
        // Use a text file named "Stars.txt" in the working directory
        try {
            using (StreamReader file = new StreamReader(starsfile_path))
            {
                string[] str;
                while ((str = TextRead.ReadCurrLine(file)) != null)
                {
                    AddText(str[0], str[1], new Vector2(float.Parse(str[2]), float.Parse(str[3])), sphere);
                }

                file.Close();
            }
        }
        catch (Exception e)
        {
            DebugMessages.Print("Could not load Stars data file! " + e.Message, DebugMessages.Colors.Error);
        }
    }

    public void AddText(string key, string text, Vector2 pos, Transform followed_sphere) {
        Text new_text = new Text();

        new_text.pos = pos;
        new_text.go = new GameObject(key);
        new_text.mesh = new_text.go.AddComponent<TextMeshProUGUI>() as TextMeshProUGUI;
        new_text.followed_sphere = followed_sphere;

        new_text.mesh.text = text;
        new_text.mesh.alignment = TextAlignmentOptions.Center;
        new_text.go.transform.SetParent(transform);

        _texts.Add(key, new_text);
    }

    public void RemoveText(string key)
    {
        if (_texts.ContainsKey(key))
        {
            GameObject.Destroy(_texts[key].go);
            _texts.Remove(key);
        }
    }

    public void AlignTextsWithSpheres() {
        foreach (KeyValuePair<string, Text> pair in _texts)
        {
            Text text = pair.Value;
            Vector3 sphere_pos = SOFConverter.EquirectangularToSphere(text.pos, TEXTSPHERE_RADIUS, text.followed_sphere);

            // Project point onto camera canvas screen
            Vector3 screen_pos = _camera.WorldToScreenPoint(sphere_pos);
            if (screen_pos.z >= 0)
            {
                text.go.SetActive(true);
                // Get where the text should be displaced
                text.pos = new Vector2(screen_pos.x, screen_pos.y);
                // Place text based on computed position
                text.go.transform.position = text.pos;
            }
            else
                text.go.SetActive(false);
        }
    }
}

struct Text {
    public Vector2 pos;             // In spherical coordinates, where the first angle is [0, 360], the second is [-90, +90]
    public TextMeshProUGUI mesh;
    public GameObject go;
    public Transform followed_sphere;
}
