namespace YTLoader.Core.Types;

public class FormatInfoQuery
{
    private readonly int _count;
    private readonly string _baseUri;
    private KeyValuePair<string, string>[] pairs;

    public FormatInfoQuery(string uri)
    {
        int divide = uri.IndexOf('?');

        if (divide == -1)
        {
            int amp = uri.IndexOf('&');
            if (amp == -1)
            {
                _baseUri = uri;
                return;
            }

            _baseUri = null;
        }
        else
        {
            _baseUri = uri.Substring(0, divide);
            uri = uri.Substring(divide + 1);
        }

        string[] keyValues = uri.Split('&');

        pairs = new KeyValuePair<string, string>[keyValues.Length];

        for (int i = 0; i < keyValues.Length; i++)
        {
            string pair = keyValues[i];
            int equals = pair.IndexOf('=');

            string key = pair.Substring(0, equals);
            string value = equals < pair.Length ? pair.Substring(equals + 1) : string.Empty;

            pairs[i] = new KeyValuePair<string, string>(key, value);
        }

        _count = keyValues.Length;
    }

    public string this[string key]
    {
        get
        {
            for (int i = 0; i < _count; i++)
            {
                var pair = pairs[i];
                if (pair.Key == key)
                    return pair.Value;
            }

            throw new KeyNotFoundException(key);
        }

        set
        {
            for (int i = 0; i < _count; i++)
            {
                var pair = pairs[i];
                if (pair.Key == key)
                {
                    pairs[i] = new KeyValuePair<string, string>(key, value);
                    return;
                }
            }

            throw new KeyNotFoundException(key);
        }
    }
}
