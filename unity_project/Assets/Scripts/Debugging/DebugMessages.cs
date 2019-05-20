using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugMessages : MonoBehaviour {

    const int MAX_LINES = 7;

    //Reference to child text
    TextMeshProUGUI _debug_text;
    int _lines = 0;

    //Constant text colors
    public enum Colors {       Neutral,                         Warning,                       Error };
    Color32[] _text_colors = { new Color32(255, 255, 255, 255), new Color32(255, 255, 0, 255), new Color32(255, 0, 0, 255) };

    public void Init()
    {
        //Get child text.
        _debug_text = transform.Find("Debug Text").gameObject.GetComponent<TextMeshProUGUI>() as TextMeshProUGUI;
    }

    public void Print(string message, Colors color)
    {
        _debug_text.text += "<color=#" + ColorUtility.ToHtmlStringRGBA(_text_colors[(int)color]) + ">" + message + "</color>\n";

        Trim();
    }

    public void PrintClear(string message, Colors color)
    {
        _debug_text.text = "<color=#" + ColorUtility.ToHtmlStringRGBA(_text_colors[(int)color]) + ">" + message + "</color>\n";

        Trim();
    }

    //Trims the first line of text if there are too many
    void Trim()
    {
        //Compute number of lines
        _lines = _debug_text.text.Split('\n').Length - 1;

        //Check if there are too many lines
        if (_lines > MAX_LINES)
        {
            _lines = MAX_LINES;
            _debug_text.text = _debug_text.text.Remove(0, _debug_text.text.IndexOf('\n') + 1);
        }
    }
}
