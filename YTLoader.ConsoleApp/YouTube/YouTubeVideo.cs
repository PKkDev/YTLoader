using System.Text;
using System.Text.Json.Nodes;

namespace YTLoader.ConsoleApp.YouTube
{
    public class YouTubeVideo
    {

        private readonly string _uri;
        private readonly YouTubeVideoQuery _uriQuery;

        private readonly string _jsPlayerUrl;

        //private string jsPlayer;

        //private bool _encrypted;
        //private bool _needNDescramble;

        //public string Title { get; }

        public int FormatCode { get; init; }


        public string VideoUrl => _uri;


        YouTubeVideoFormat InfoFromUrl { get; init; }
        YouTubeVideoFormat InfoFromResponse { get; init; }


        public int Fps { get; set; }
        public int AudioBitrate { get; set; }
        public int Resolution { get; set; }
        public VideoFormat Format { get; set; }
        public AudioFormat AudioFormat { get; set; }
        public AdaptiveKind AdaptiveKind { get; set; }

        public string FileExtension
        {
            get
            {
                return Format switch
                {
                    VideoFormat.Mp4 => ".mp4",
                    VideoFormat.WebM => ".webm",
                    VideoFormat.Unknown => string.Empty,
                    _ => throw new NotImplementedException($"Format {Format} is unrecognized! Please file an issue at libvideo on GitHub."),
                };
            }
        }


        public YouTubeVideo(UnscrambledQuery query, string jsPlayerUrl, JsonNode item)
        {
            _uri = query.Uri;
            _uriQuery = new YouTubeVideoQuery(query.Uri);

            _jsPlayerUrl = jsPlayerUrl;

            //this._encrypted = query.IsEncrypted;
            //this._needNDescramble = _uriQuery.ContainsKey("n");
            FormatCode = int.Parse(_uriQuery["itag"]);

            InfoFromUrl = new(FormatCode);
            InfoFromResponse = new(item);

            Fps = YouTubeVideoFormats.GetFps(FormatCode);
            AudioBitrate = YouTubeVideoFormats.GetAudioBitrate(FormatCode);
            Resolution = YouTubeVideoFormats.GetResolution(FormatCode);
            Format = YouTubeVideoFormats.GetVideoFormat(FormatCode);
            AudioFormat = YouTubeVideoFormats.GetAudioFormat(FormatCode);
            AdaptiveKind = YouTubeVideoFormats.GetAdaptiveKind(FormatCode);
        }
    }

    public class YouTubeVideoQuery
    {
        private readonly int _count;
        private readonly string _baseUri;
        private KeyValuePair<string, string>[] pairs;

        public YouTubeVideoQuery(string uri)
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
}
