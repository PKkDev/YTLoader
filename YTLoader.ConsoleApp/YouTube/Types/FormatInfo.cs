using System.Text.Json.Nodes;
using YTLoader.ConsoleApp.YouTube.Enums;

namespace YTLoader.ConsoleApp.YouTube.Types
{
    public class FormatInfo
    {
        public readonly string VideoUrl;

        private readonly FormatInfoQuery _uriQuery;

        public int FormatCode { get; init; }
        public YouTubeVideoFormat InfoFromUrl { get; init; }

        public YouTubeVideoFormat InfoFromResponse { get; init; }

        public FormatInfo(UnscrambledQuery query, JsonNode item)
        {
            VideoUrl = query.Uri;
            _uriQuery = new FormatInfoQuery(query.Uri);

            FormatCode = int.Parse(_uriQuery["itag"]);
            InfoFromUrl = new(FormatCode);

            InfoFromResponse = new(item);
        }

        public string FileExtension
        {
            get
            {
                return InfoFromUrl.Format switch
                {
                    VideoFormat.Mp4 => ".mp4",
                    VideoFormat.WebM => ".webm",
                    VideoFormat.Unknown => string.Empty,
                    _ => throw new NotImplementedException($"Format {InfoFromUrl.Format} is unrecognized! Please file an issue at libvideo on GitHub."),
                };
            }
        }
    }
}
