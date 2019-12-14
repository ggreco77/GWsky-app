using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SphereText : MonoBehaviour
{
    const float TEXTSPHERE_RADIUS = 9.0f;

    Dictionary<string, Text> _texts = new Dictionary<string, Text>();
    Camera _camera;

    public void Init(Camera main_camera)
    {
        _camera = main_camera;
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
            float az = (float)MathExtension.ToRadians(text.pos.y);
            float h = (float)MathExtension.ToRadians(text.pos.x);
            // Get point position on the sphere as cartesian coordinates
            Vector3 sphere_pos = TEXTSPHERE_RADIUS * new Vector3(Mathf.Cos(-h + Mathf.PI/2),
                                                                 -Mathf.Sin(-h + Mathf.PI/2) * Mathf.Sin(az),
                                                                 -Mathf.Sin(-h + Mathf.PI/2) * Mathf.Cos(az));
            // Make an initial orientation for lineup
            sphere_pos = Quaternion.Euler(0, 90, 0) * sphere_pos;
            sphere_pos = Quaternion.Euler(-90, 0, 0) * sphere_pos;
            sphere_pos = Quaternion.Euler(0, 0, 180) * sphere_pos;
            // Rotate points as per sphere orientation
            sphere_pos = text.followed_sphere.rotation * sphere_pos;
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
