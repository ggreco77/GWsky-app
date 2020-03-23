using UnityEngine;

/// <summary>
/// Static class used to print debugging messages on the top-left corner of the screen.
/// </summary>
static class Log {
    // Maximum number of lines displayed at any one time
    const int MAX_LINES = 10;
    // Whether debugging mode for debug messages is enabled
    static public bool Enabled { get; set; } = true;

    // Reference to current log text
    static public string Text { get; private set; } = "";
    // Initial line to print, if only more recent information need to be displayed
    static public int StartLine { get; private set; } = 0;

    //Constant text colors as a public enum and corresponding RGBA colors
    public enum Colors {                       Neutral,                         Warning,                       Error };
    static readonly Color32[] _text_colors = { new Color32(255, 255, 255, 255), new Color32(255, 255, 0, 255), new Color32(255, 0, 0, 255) };

    /// <summary>
    /// Prints a debug message with a given color.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="color"></param>
    public static void Print(string message, Colors color = Colors.Neutral) {
        // Append the text as a new line using the input color (via Rich Text)
        Text += "<color=#" + ColorUtility.ToHtmlStringRGBA(_text_colors[(int)color]) + ">" + message + "</color>\n";

        // Trim lines in excess
        Trim();
    }

    /// <summary>
    /// Clears the debug messages texts, then writes a debug message with a given color.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="color"></param>
    public static void PrintClear(string message, Colors color = Colors.Neutral) {
        // Set the initial line as the number of lines (no remaining text)
        StartLine = Text.Split('\n').Length - 1;
        
        // Print the required debug message
        Print(message, color);
    }

    /// <summary>
    /// Trims lines of text so that the number of lines in the text is at most MAX_LINES.
    /// </summary>
    /// Trimming starts from the least recent messages (topmost message lines).
    static void Trim() {
        //Compute the number of lines in the debug messages text
        int lines = Text.Split('\n').Length - StartLine - 1;

        // As long as there are more lines than the maximum allowed...
        while (lines > MAX_LINES) {
            // Ignore text up until another newline
            StartLine++;
            // Recompute number of lines
            lines = Text.Split('\n').Length - StartLine - 1;
        }
    }
}
