using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Text;

/// <summary>
/// Component which allows text to be displayed and aligned above the photosphere.
/// </summary>
/// GameObject with this component should also have a Canvas component.
public class SphereText : MonoBehaviour {
    // Radius of the reference sphere in which to project texts
    const float TEXTSPHERE_RADIUS = 5.0f;
    // Font size of all texts. Actual size on screen is determined by Canvas scale
    const int TEXT_SIZE = 15;

    // List of texts to display, as a pair of (ID, text structure)
    readonly Dictionary<string, Text> _texts = new Dictionary<string, Text>();
    // In-game camera
    Camera _camera;

    /// <summary>
    /// Init method, should be called by Master. Unfortunately Constructors do not work well with Unity, so a Init function has to be called.
    /// </summary>
    /// <param name="main_camera"></param>
    public void Init(Camera main_camera) {
        // Pass the in-game camera to a local variable
        _camera = main_camera;
    }

    /// <summary>
    /// Loads a list of texts to display at given coordinates from a text file.
    /// </summary>
    /// Folder and file name need to correspond to Unity's hierarchy of the Resources folder tree.
    /// <param name="folder">Folder on the local storage in which the text file is contained</param>
    /// <param name="filename">Name of the file (Without extension!)</param>
    /// <param name="followed_sphere">Sphere that these texts should be aligned with</param>
    public void LoadFromTextFile(Transform followed_sphere) {
        try {
            // Get Stars file from Resources in Unity
            string stars_string = Resources.Load<TextAsset>("Data/Stars").text;
            // Instantiate a reader for the input file
            StreamReader file = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(stars_string)));
            // Array of strings where the first element is the text ID, the second the text itself, the third and fourth the position in spherical coordinates, the fifth
            // the declination offset
            string[] str;
            // While there are lines to read, read a line...
            while ((str = TextRead.ReadCurrLine(file)) != null)
                // ... And create a new text from the info given in the non-empty line
                AddText(str[0], str[1], new Vector2(float.Parse(str[2]), float.Parse(str[3])), followed_sphere);

            // Close the input file
            file.Close();
        }
        // If for some reason an exception is raised...
        catch (Exception e) {
            // Print an error to debug with the error message
            Log.Print("Could not load Stars data file! " + e.Message, Log.Colors.Error);
        }
    }

    /// <summary>
    /// Add a new text to the list of texts to align and display.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="text"></param>
    /// <param name="pos"></param>
    /// <param name="followed_sphere"></param>
    public void AddText(string key, string text, Vector2 pos, Transform followed_sphere) {
        // Create a new text structure, passing the spherical position, offset and creating a new GameObject with
        // name equal to the provided ID
        Text new_text = new Text {
            pos = pos,
            go = new GameObject(key)
        };

        // Add a new text mesh to the GameObject
        new_text.mesh = new_text.go.AddComponent<TextMeshProUGUI>();
        // Set the sphere to which to align the text as the input sphere
        new_text.followed_sphere = followed_sphere;

        // Set the text of the text mesh as the input text
        new_text.mesh.text = text;
        // Set the font
        new_text.mesh.font = Resources.Load<TMP_FontAsset>("Fonts/nasalization-rg");
        // Set the text size as a constant
        new_text.mesh.fontSize = TEXT_SIZE;
        // Set the text pivot
        RectTransform text_rt = (RectTransform) new_text.go.transform;
        text_rt.pivot = new Vector2(-0.025f, -1);
        // Set the text height
        text_rt.sizeDelta = new Vector2(200, 1);
        // Set text not to wrap on a new line
        new_text.mesh.enableWordWrapping = false;
        // Set text to be centered on its origin
        new_text.mesh.alignment = TextAlignmentOptions.Left;
        // Set parent of the text as the object possessing this component (which should also have a Canvas)
        new_text.go.transform.SetParent(transform);

        // Add the newly created text to the list of texts
        _texts.Add(key, new_text);
    }

    /// <summary>
    /// Remove an existing text from the list of texts to align and display, given its ID.
    /// </summary>
    /// <param name="key"></param>
    public void RemoveText(string key) {
        // If the list of texts contains a text with the input ID...
        if (_texts.ContainsKey(key)) {
            // Destroy the GameObject of that text
            GameObject.Destroy(_texts[key].go);
            // Remove the text with the input ID from the list of texts
            _texts.Remove(key);
        }
    }

    /// <summary>
    /// Align texts with their followed sphere.
    /// </summary>
    void AlignTextsWithSphere() {
        // For all texts in the list...
        foreach (KeyValuePair<string, Text> pair in _texts) {
            // Fetch the text from the pair of ID and text structure
            Text text = pair.Value;
            // Find the point on the followed sphere corresponding to the spherical coordinates of the text
            Vector3 sphere_pos = SOFConverter.EquirectangularToSphere(text.pos, text.followed_sphere, TEXTSPHERE_RADIUS);
            // Set the text GameObject as active and visible
            text.go.SetActive(true);
            // Place the text based on computed screen position
            text.go.transform.position = sphere_pos;

            text.go.transform.rotation = _camera.transform.rotation;
        }

        GetComponent<Canvas>().transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
    }

    /// <summary>
    /// Monobehaviour Method. LateUpdate is called once per frame, after all Update calls.
    /// </summary>
    void LateUpdate() {
        // Align texts
        AlignTextsWithSphere();
    }
}

/// <summary>
/// Structure containing data for a text aligned with the spheres.
/// </summary>
struct Text {
    public Vector2 pos;                 // Position of the text in spherical coordinates, where the first angle is [0, 360], the second is [-90, +90]
    public TextMeshProUGUI mesh;        // The text component
    public GameObject go;               // Gameobject containing the text
    public Transform followed_sphere;   // Sphere that the text should be aligned with
}
