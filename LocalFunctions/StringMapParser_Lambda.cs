using System.Text;

namespace csharpfun.LocalFunctions;

public class StringMapParser_Lambda
{
    public static IReadOnlyDictionary<string, string> Parse(string raw)
    {
        Dictionary<string, string> result = new();
        int index = 0;

        var readDelimiter = (char c) =>
        {
            if (index >= raw.Length) throw new ParseException("Unexpectedly reached end of string.");
            if (c != raw[index]) throw new ParseException("Unexpected character found.");
            index++;
        };

        var readString = () =>
        {
            StringBuilder key = new StringBuilder();
            for (; index < raw.Length && char.IsLetter(raw[index]); index++)
            {
                key.Append(raw[index]);
            }
            return key.ToString();
        };

        var parsePair = () =>
        {
            string key = readString();
            readDelimiter('=');
            string value = readString();

            if (!result.TryAdd(key, value))
            {
                throw new ParseException("Key already present.");
            }
        };

        var parseImpl = () =>
        {
            while (index < raw.Length)
            {
                parsePair();
                readDelimiter(';');
            }
        };

        parseImpl();

        return result;
    }
}