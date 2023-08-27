using System.Text;

namespace YTLoader.Core.Types;

public class VideoInfo
{
    public string Title { get; init; }

    public int? LengthSeconds { get; init; }

    public string Author { get; init; }

    public string VideoId { get; init; }

    public List<string> Keywords { get; init; }

    public List<FormatInfo> FormatsInfo { get; init; }

    public string PlayerData { get; init; }

    public string JsPlayer { get; init; }

    public VideoInfo(
        string? title, int? second, string? author, string? videoId, List<string> keywords, string playerData, string jsPlayer)
    {
        Title = title ?? "none";
        LengthSeconds = second;
        Author = author ?? "none";
        VideoId = videoId ?? "none";
        Keywords = keywords;
        PlayerData = playerData;

        JsPlayer = jsPlayer;

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
