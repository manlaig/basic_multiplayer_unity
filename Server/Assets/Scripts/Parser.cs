using UnityEngine;
using System.Text.RegularExpressions;

public static class Parser
{
    public static int ParseSequenceNumber(string data)
    {
        int seqNumber = -1;
        Match match = Regex.Match(data, @"(?<seqNumber>\d+) c\d+t");
        int.TryParse(match.Groups["seqNumber"].Value, out seqNumber);
        return seqNumber;
    }

    public static string ParseID(string data)
    {
        Match match = Regex.Match(data, @"(?<id>c\d+t)");
        return match.Groups["id"].Value;
    }

    public static string ParseInput(string data)
    {
        Match match = Regex.Match(data, @"c\d+t (?<input>[adws])");
        return match.Groups["input"].Value;
    }

    public static Vector3 ParseInitialPosition(string data)
    {
        Match match = Regex.Match(data,
            @"n (?<x>-?([0-9]*[.])?[0-9]+) (?<y>-?([0-9]*[.])?[0-9]+) (?<z>-?([0-9]*[.])?[0-9]+)");
        return ParsePosition(match);
    }

    public static Vector3 ParsePosition(Match match)
    {
        float x, y, z;
        float.TryParse(match.Groups["x"].Value, out x);
        float.TryParse(match.Groups["y"].Value, out y);
        float.TryParse(match.Groups["z"].Value, out z);
        return new Vector3(x, y, z);
    }
}
