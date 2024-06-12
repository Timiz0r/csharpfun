using System.Text;

namespace csharpfun.LocalFunctions;

public class StringMapParser_LocalFunc
{
    public static IReadOnlyDictionary<string, string> Parse(string raw)
    {
        Dictionary<string, string> result = new();
        int index = 0;

        ParseImpl();

        return result;

        void ParseImpl()
        {
            while (index < raw.Length)
            {
                ParsePair();
                ReadDelimiter(';');
            }
        }

        void ParsePair()
        {
            string key = ReadString();
            ReadDelimiter('=');
            string value = ReadString();

            if (!result.TryAdd(key, value))
            {
                throw new ParseException("Key already present.");
            }
        }

        void ReadDelimiter(char c)
        {
            if (index >= raw.Length) throw new ParseException("Unexpectedly reached end of string.");
            if (c != raw[index]) throw new ParseException("Unexpected character found.");
            index++;
        }

        string ReadString()
        {
            StringBuilder key = new StringBuilder();
            for (; index < raw.Length && char.IsLetter(raw[index]); index++)
            {
                key.Append(raw[index]);
            }
            return key.ToString();
        }
    }
}