using System.IO.Pipes;

namespace PixelWallE.Core;

public class Token
{
    public string Type { get; }
    public string Value { get; }
    public int Line { get; }

    public Token(string type, string value, int line)
    {
        Type = type;
        Value = value;
        Line = line;
    }
}