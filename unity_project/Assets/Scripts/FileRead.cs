using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FileRead {

    static string NullEvaluate(string input_str)
    {
        if (input_str == "null")
            return null;
        else
            return input_str;
    }

    public static string[] ReadCurrLine(StreamReader reader)
    {
        List<string> list = new List<string>();
        SkipEmptyLines(reader);

        string str = reader.ReadLine();

        if (System.String.IsNullOrEmpty(str))
            return null;

        var forbiddenChars = new char[] { '\n', '\r' };
        str = new string(str.Where(c => !forbiddenChars.Contains(c)).ToArray());

        string _temp = null;
        
        if (str[0] != '\t')
        {
            int index = str.IndexOf(':');
            if (index != -1)
            {
                _temp = str.Substring(0, str.IndexOf(':'));
                list.Add(NullEvaluate(_temp));
                str = str.Remove(0, _temp.Length + 1);
            }
        }
        else
            list.Add(null);

        int limiter = 0;
        while (!System.String.IsNullOrEmpty(str))
        {
            limiter++;
            while (str[0] == ' ' || str[0] == '\t')
                str = str.Remove(0, 1);

            if (str[0] != '\"' && str.IndexOf(',') != -1)
            {
                if (str.IndexOf(',') > 0)
                {
                    _temp = str.Substring(0, str.IndexOf(','));
                    forbiddenChars = new char[] { ' ', '\t' };
                    _temp = new string(_temp.Where(c => !forbiddenChars.Contains(c)).ToArray());
                    list.Add(NullEvaluate(_temp));
                    str = str.Remove(0, _temp.Length + 2);
                }
                else
                {
                    forbiddenChars = new char[] { ' ', '\t' };
                    str = new string(str.Where(c => !forbiddenChars.Contains(c)).ToArray());
                    list.Add(NullEvaluate(str));
                    str = string.Empty;
                }
            }
            else if (str[0] == '\"')
            {
                str = str.Remove(0, 1);
                _temp = str.Substring(0, str.IndexOf('\"'));
                list.Add(NullEvaluate(_temp));
                str = str.Remove(0, Mathf.Min(_temp.Length + 3, str.Length));
            }
            else
            {
                list.Add(NullEvaluate(str));
                str = string.Empty;
            }
        }

        return list.ToArray();
    }

    public static void SkipEmptyLines(StreamReader reader)
    {
        int peek;
        while (true)
        {
            peek = reader.Peek();
            if (peek != '\n' && peek != '\r')
                break;
            reader.Read();
        }
    }

}
