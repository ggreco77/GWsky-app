using System.IO;
using System.Collections.Generic;

/// <summary>
/// Static class containing methods for reading a formatted line of data from a text file.
/// </summary>
/// Format of the text lines should be as follows:
///     {ID}: {values separated by commas}
/// Or as follows:
/// {values separated by commas}
/// Comment lines begin with an asterisk "*". Comments can also be started mid-line.
/// Eventual whitespaces are ignored. For strings with whitespaces, colons or commas, double quotes MUST be used.
/// Characters ':', ',', '\"', "*" as well as any return characters should only be used within double quotes or for their purpose as
/// explained in the format.
public static class TextRead {
    /// <summary>
    /// Evaluates whether a value is to be considered null.
    /// </summary>
    /// <param name="input_str"></param>
    /// <returns></returns>
    static string NullEvaluate(string input_str) {
        // A value in the text file written as the string "null" is considered not a valid string, but a null value. So is an empty string
        if (input_str == "null" || input_str == "")
            return null;
        else
        // In all other cases, the string is considered as is
            return input_str;
    }

    /// <summary>
    /// Advances the input reader to the next non-whitespace character (or End Of File).
    /// </summary>
    /// <param name="reader"></param>
    public static void SkipEmptyLines(StreamReader reader) {
        // Next character in the stream
        int peek;
        // Condition to exit from cycle
        bool exit = false;

        // Cycle until a condition is met...
        while (!exit) {
            // Peek the next character in the stream. This does not advance the stream
            peek = reader.Peek();
            // If the peeked character is not a whitespace or an asterisk, or if it is the end of file (-1)...
            if (!char.IsWhiteSpace((char)peek) && peek != '*' || peek == -1)
                // Exit the cycle
                exit = true;
            // Otherwise, if the character is an asterisk, skip the entire comment line
            else if (peek == '*')
                reader.ReadLine();
            // Otherwise, skip the whitespace character
            else
                reader.Read();
        }
    }

    /// <summary>
    /// Read a line from the input text file with proper formatting, returning an array of strings as follows:
    /// {ID}     -> elem 0 [null if there is no ID]
    /// {value1} -> elem 1
    /// {value2} -> elem 2
    /// ... and so on.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static string[] ReadCurrLine(StreamReader reader) {
        // List of strings to be returned
        List<string> list = new List<string>();

        // Skip all empty lines until a readable line is found
        SkipEmptyLines(reader);

        // Character read at the current loop iteration
        int curr_char;
        // Current string value, built one character at a time
        string curr_value = "";
        // Whether special characters should be inserted in the current string value due to
        // opened double quotes
        bool inside_quotes = false,
             // Whether the end of line has been reached
             end = false,
             // Whether a string value has been completed and only whitespaces, a comma or a colon
             // should come next
             expected_comma = false;

        // If the End Of File has been reached without any valid character, return a null array
        if (reader.Peek() == -1)
            return null;

        // While the end-of-line condition has not been reached...
        while (!end) {
            // Read a single character
            curr_char = reader.Read();

            // If the read character is the End Of File...
            if (curr_char == -1) {
                // If the list is empty, add an empty identifier value
                if (list.Count == 0)
                    list.Add(null);
                // Add the current string value
                list.Add(NullEvaluate(curr_value));
                // Set to exit the cycle
                end = true;
                // If still inside double quotes, it means quotes were opened but never closed. Throw an exception
                if (inside_quotes)
                    throw new ReadingFormatException("In line reading, End Of File has been reached and quotes have never been closed!");
            }
            // Otherwise, if double quotes have been opened...
            else if (inside_quotes) {
                // If the character is a double quote...
                if (curr_char == '\"')
                    // ... quotes have been closed
                    inside_quotes = false;
                // Otherwise, whatever character it is should be added to the value string (even if it is a comma, colon,
                // whitespace or special char)
                else
                    curr_value += (char)curr_char;
            }
            // Otherwise, if an asterisk has been found mid-sentence...
            else if (curr_char == '*') {
                // Add the current string value
                list.Add(NullEvaluate(curr_value));
                // Set to exit the cycle. If the entire line is a comment, it is simply ignored
                end = true;
                // Read the rest of the line
                reader.ReadLine();
            }
            // Otherwise, if an end of line character is found...
            else if (curr_char == '\n' || curr_char == '\r') {
                // If the list is empty, add an empty identifier value
                if (list.Count == 0)
                    list.Add(null);
                // Add the current string value
                list.Add(NullEvaluate(curr_value));
                // Set to exit the cycle
                end = true;
            }
            // Otherwise, if a whitespace character is found (besides line ends)...
            else if (char.IsWhiteSpace((char)curr_char)) {
                // If some characters have been written onto the value string...
                if (curr_value != "")
                    // ... expect only more whitespaces before a comma or colon
                    expected_comma = true;
            }
            // Otherwise, if a double quote character is found...
            else if (curr_char == '\"') {
                // Set double quotes as opened
                inside_quotes = true;
            }
            // Otherwise, if the current character is a comma...
            else if (curr_char == ',') {
                // If the list is empty, add an empty identifier value
                if (list.Count == 0)
                    list.Add(null);
                // Add the current string value
                list.Add(NullEvaluate(curr_value));
                // Empty the current value
                curr_value = "";
                // Expect any character from now on
                expected_comma = false;
            }
            // Otherwise, if the current character is a colon...
            else if (curr_char == ':') {
                // If the list is empty...
                if (list.Count == 0) {
                    // Add the current string value
                    list.Add(NullEvaluate(curr_value));
                    // Empty the current value
                    curr_value = "";
                    // Expect any character from now on
                    expected_comma = false;
                }
                else
                // Otherwise, since the colon should separate the identifier, the format is clearly wrong
                    throw new ReadingFormatException("In line reading, colon should only be applied for identifier!");
            }
            // Finally, if the character is any character except those above...
            else {
                // If a comma or colon was expected, it means there are two values without separation. Throw a format exception
                if (expected_comma)
                    throw new ReadingFormatException("In line reading, expected a comma, colon or line end but found a non-whitespace character!");
                else
                // Otherwise, everything is fine. Add the character to the value string
                    curr_value += (char)curr_char;
            }
        }

        // Once the cycle has ended, return the value strings in an array
        return list.ToArray();
    }
}
