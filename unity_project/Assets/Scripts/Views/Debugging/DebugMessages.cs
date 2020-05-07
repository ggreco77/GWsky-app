using UnityEngine;
using TMPro;

/// <summary>
/// Displays the most recent log messages to screen.
/// </summary>
public class DebugMessages : MonoBehaviour {
    // Reference to text component in which to write text
    TextMeshProUGUI _text;

    /// <summary>
    /// Monobehaviour Method. Start is called before the first frame.
    /// </summary>
    void Start() {
        // Link text reference to its component in the same GameObject
        _text = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Monobehaviour Method. Update is called once per frame.
    /// </summary>
    void Update() {
        // Fetch full log text from Log class
        string raw_text = Log.Text;

        // Remove a line of text at a time until a sufficient amount of lines has been removed,
        // so as to only show the latest log lines
        for (int i = 0; i < Log.StartLine; i++) {
            raw_text = raw_text.Remove(0, raw_text.IndexOf("\n") + 1);
        }

        // Display the text, if debugging is enabled
        if (Options.DebugPrint)
            _text.text = raw_text;
        else
            _text.text = "";
    }
}
