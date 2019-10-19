using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class DebugMessages {

    const int MAX_LINES = 10;

    //Reference to child text
    static TextMeshProUGUI _debug_text;
    static int _lines = 0;

    static bool _initiated = false;

    //Constant text colors
    public enum Colors {       Neutral,                         Warning,                       Error };
    static Color32[] _text_colors = { new Color32(255, 255, 255, 255), new Color32(255, 255, 0, 255), new Color32(255, 0, 0, 255) };

    public static void Init()
    {
        //Get child text.
        _debug_text = GameObject.Find("Debug Messages Canvas/Debug Text").gameObject.GetComponent<TextMeshProUGUI>() as TextMeshProUGUI;
        _initiated = true;
    }

    public static void Print(string message, Colors color = DebugMessages.Colors.Neutral)
    {
        if (!_initiated)
            Init();
        _debug_text.text += "<color=#" + ColorUtility.ToHtmlStringRGBA(_text_colors[(int)color]) + ">" + message + "</color>\n";

        Trim();
    }

    public static void PrintClear(string message, Colors color = DebugMessages.Colors.Neutral)
    {
        if (!_initiated)
            Init();
        _debug_text.text = "<color=#" + ColorUtility.ToHtmlStringRGBA(_text_colors[(int)color]) + ">" + message + "</color>\n";

        Trim();
    }

    //Trims the first line of text if there are too many
    static void Trim()
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
