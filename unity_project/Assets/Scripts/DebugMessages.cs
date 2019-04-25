using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugMessages : MonoBehaviour {

    //Reference to child text
    TextMeshProUGUI _debug_text;

    //Constant text colors
    public enum Colors {       Neutral,                         Warning };
    Color32[] _text_colors = { new Color32(255, 255, 255, 255), new Color32(255, 255, 0, 255) };

    public void Init()
    {
        //Get child text.
        _debug_text = transform.Find("Debug Text").gameObject.GetComponent<TextMeshProUGUI>() as TextMeshProUGUI;
    }

    public void Print(string message, Colors color)
    {
        _debug_text.text = message;
        _debug_text.color = _text_colors[(int) color];
    }
}
