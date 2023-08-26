using System.Text.Json.Nodes;

namespace YTLoader.ConsoleApp.YouTube;

public class YouTubeVideoFormat
{
    public int Fps { get; set; }

    public int AudioBitrate { get; set; }

    public int Resolution { get; set; }

    public VideoFormat Format { get; set; }

    public AudioFormat AudioFormat { get; set; }

    public AdaptiveKind AdaptiveKind { get; set; }

    public YouTubeVideoFormat(int formatCode)
    {
        Fps = YouTubeVideoFormats.GetFps(formatCode);
        AudioBitrate = YouTubeVideoFormats.GetAudioBitrate(formatCode);
        Resolution = YouTubeVideoFormats.GetResolution(formatCode);
        Format = YouTubeVideoFormats.GetVideoFormat(formatCode);
        AudioFormat = YouTubeVideoFormats.GetAudioFormat(formatCode);
        AdaptiveKind = YouTubeVideoFormats.GetAdaptiveKind(formatCode);
    }

    public YouTubeVideoFormat(JsonNode formatNode)
    {

        var mimeType = formatNode["mimeType"]?.GetValue<string>();
        var bitrate = formatNode["bitrate"]?.GetValue<int>();
        var width = formatNode["width"]?.GetValue<int>();
        var height = formatNode["height"]?.GetValue<int>();
        var fps = formatNode["fps"]?.GetValue<int>();
        var qualityLabel = formatNode["qualityLabel"]?.GetValue<string>();
    }
}
