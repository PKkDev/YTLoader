using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using YTLoader.ConsoleApp.YouTube.Enums;
using YTLoader.ConsoleApp.YouTube.Helpers;

namespace YTLoader.ConsoleApp.YouTube.Types;

public class YouTubeVideoFormat
{
    public int Fps { get; set; }

    public int AudioBitrate { get; set; }

    public int Resolution { get; set; }

    public string QualityLabel { get; set; }

    public string MimeType { get; set; }

    public List<string> Codecs { get; set; }

    public VideoFormat Format { get; set; }

    public AudioFormat AudioFormat { get; set; }

    public AdaptiveKind AdaptiveKind { get; set; }

    public YouTubeVideoFormat()
    {
        Fps = -1;
        AudioBitrate = -1;
        Resolution = -1;
        QualityLabel = "none";
        MimeType = "none";
        Format = VideoFormat.Unknown;
        AudioFormat = AudioFormat.Unknown;
        AdaptiveKind = AdaptiveKind.None;
    }

    public YouTubeVideoFormat(int formatCode)
        : this()
    {
        Fps = FormatHelper.GetFps(formatCode);
        AudioBitrate = FormatHelper.GetAudioBitrate(formatCode);
        Resolution = FormatHelper.GetResolution(formatCode);
        Format = FormatHelper.GetVideoFormat(formatCode);
        AudioFormat = FormatHelper.GetAudioFormat(formatCode);
        AdaptiveKind = FormatHelper.GetAdaptiveKind(formatCode);
    }

    public YouTubeVideoFormat(JsonNode formatNode)
        : this()
    {
        var fps = formatNode["fps"]?.GetValue<int>();
        if (fps != null)
            Fps = (int)fps;

        var bitrate = formatNode["bitrate"].GetValue<int>();
        AudioBitrate = bitrate;

        var height = formatNode["height"]?.GetValue<int>();
        var width = formatNode["width"]?.GetValue<int>();
        if (height != null)
            Resolution = (int)height;

        var mimeType = formatNode["mimeType"]?.GetValue<string>();
        Console.WriteLine(mimeType);
        if (mimeType != null)
        {
            MimeType = mimeType;

            var args = mimeType.Split(';');
            if (args.Length > 0)
            {
                var format = args[0].Trim().Split('/');
                switch (format[0])
                {
                    case "video": AdaptiveKind = AdaptiveKind.Video; break;
                    case "audio": AdaptiveKind = AdaptiveKind.Audio; break;
                }
                switch (format[1])
                {
                    case "webm": Format = VideoFormat.WebM; break;
                    case "mp4": Format = VideoFormat.Mp4; break;
                }

                var codecsMatch = Regex.Match(mimeType, "codecs=\"(.*)\"");
                if (codecsMatch.Success && codecsMatch.Groups.Count > 1)
                {
                    var value = codecsMatch.Groups[1].Value;
                    var codecs = value.Split(",");
                    Codecs = codecs.ToList();
                }
            }



            //if (mimeType.Contains("video", StringComparison.OrdinalIgnoreCase))
            //    AdaptiveKind = AdaptiveKind.Video;
            //if (mimeType.Contains("audio", StringComparison.OrdinalIgnoreCase))
            //    AdaptiveKind = AdaptiveKind.Audio;

            //if (mimeType.Contains("webm", StringComparison.OrdinalIgnoreCase))
            //    Format = VideoFormat.WebM;
            //if (mimeType.Contains("mp4", StringComparison.OrdinalIgnoreCase))
            //    Format = VideoFormat.Mp4;

            //if (mimeType.Contains("Aac", StringComparison.OrdinalIgnoreCase))
            //    AudioFormat = AudioFormat.Aac;
            //if (mimeType.Contains("Vorbis", StringComparison.OrdinalIgnoreCase))
            //    AudioFormat = AudioFormat.Vorbis;
            //if (mimeType.Contains("Opus", StringComparison.OrdinalIgnoreCase))
            //    AudioFormat = AudioFormat.Opus;
        }

        var qualityLabel = formatNode["qualityLabel"]?.GetValue<string>();
        QualityLabel = qualityLabel ?? "none";
    }
}


/*
 
 
 
video/mp4; codecs="avc1.42001E, mp4a.40.2"
video/mp4; codecs="avc1.64001F, mp4a.40.2"
video/mp4; codecs="avc1.4d401f"
video/webm; codecs="vp9"
video/mp4; codecs="av01.0.05M.08"
video/mp4; codecs="avc1.4d401e"
video/webm; codecs="vp9"
video/mp4; codecs="av01.0.04M.08"
video/mp4; codecs="avc1.4d401e"
video/webm; codecs="vp9"
video/mp4; codecs="av01.0.01M.08"
video/mp4; codecs="avc1.4d4015"
video/webm; codecs="vp9"
video/mp4; codecs="av01.0.00M.08"
video/mp4; codecs="avc1.4d400c"
video/webm; codecs="vp9"
video/mp4; codecs="av01.0.00M.08"
audio/mp4; codecs="mp4a.40.2"
audio/webm; codecs="opus"
audio/webm; codecs="opus"
audio/webm; codecs="opus"
 
 
 
 */