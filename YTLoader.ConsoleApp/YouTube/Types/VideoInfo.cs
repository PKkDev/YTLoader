using System.Text;

namespace YTLoader.ConsoleApp.YouTube.Types
{
    public class VideoInfo
    {
        public string Title { get; init; }

        public int? LengthSeconds { get; init; }

        public string Author { get; init; }

        public string VideoId { get; init; }

        public List<string> Keywords { get; init; }

        public List<FormatInfo> FormatsInfo { get; init; }

        public string PlayerData { get; init; }

        public VideoInfo(
            string? title, int? second, string? author, string? videoId, List<string> keywords, string playerData)
        {
            Title = title ?? "no information";
            LengthSeconds = second;
            Author = author ?? "no information";
            VideoId = videoId ?? "no information";
            Keywords = keywords;
            PlayerData = playerData;

            FormatsInfo = new();
        }

        public string VideoName
        {
            get
            {
                StringBuilder builder = new(Title);

                foreach (char bad in Path.GetInvalidFileNameChars())
                    builder.Replace(bad, '_');

                return builder.ToString();
            }
        }

    }
}
