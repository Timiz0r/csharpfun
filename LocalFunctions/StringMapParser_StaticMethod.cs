using System.Text;

namespace csharpfun.LocalFunctions;

public class StringMapParser_StaticMethod
{
    public static IReadOnlyDictionary<string, string> Parse(string raw)
    {
        Dictionary<string, string> result = new();
        int index = 0;

        ParseImpl(raw, result, ref index);

        return result;
    }

    private static void ParseImpl(string raw, Dictionary<string, string> result, ref int index)
    {
        while (index < raw.Length)
        {
            ParsePair(raw, result, ref index);
            ReadDelimiter(';', raw, ref index);
        }
    }

    private static void ParsePair(string raw, Dictionary<string, string> result, ref int index)
    {
        string key = ReadString(raw, ref index);
        ReadDelimiter('=', raw, ref index);
        string value = ReadString(raw, ref index);

        if (!result.TryAdd(key, value))
        {
            throw new ParseException("Key already present.");
        }
    }

    private static void ReadDelimiter(char c, string raw, ref int index)
    {
        if (index >= raw.Length) throw new ParseException("Unexpectedly reached end of string.");
        if (c != raw[index]) throw new ParseException("Unexpected character found.");
        index++;
    }

    private static string ReadString(string raw, ref int index)
    {
        StringBuilder key = new StringBuilder();
        for (; index < raw.Length && char.IsLetter(raw[index]); index++)
        {
            key.Append(raw[index]);
        }
        return key.ToString();
    }
}