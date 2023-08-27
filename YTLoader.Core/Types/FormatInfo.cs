using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using YTLoader.Core.Enums;
using YTLoader.Core.Helpers;

namespace YTLoader.Core.Types;

public class FormatInfo
{
    public readonly string VideoUrl;

    //private readonly FormatInfoQuery _uriQuery;

    #region YouTubeVideoFormat

    public int FormatCode { get; set; }

    public int Fps { get; set; }

    public int AudioBitrate { get; set; }

    public int Resolution { get; set; }

    public string QualityLabel { get; set; }

    public string MimeType { get; set; }

    public List<string> Codecs { get; set; }

    public VideoFormat Format { get; set; }

    public AudioFormat AudioFormat { get; set; }

    public AdaptiveKind AdaptiveKind { get; set; }

    public string? AudioQuality { get; set; }

    #endregion YouTubeVideoFormat

    public FormatInfo()
    {
        Fps = -1;
        AudioBitrate = -1;
        Resolution = -1;
        QualityLabel = "none";
        MimeType = "none";
        Codecs = new();
        Format = VideoFormat.Unknown;
        AudioFormat = AudioFormat.Unknown;
        AdaptiveKind = AdaptiveKind.None;
        AudioQuality = "none";
    }


    public FormatInfo(UnscrambledQuery query, JsonNode item)
        : this()
    {
        VideoUrl = query.Uri;

        MapFormat(item);

        //_uriQuery = new FormatInfoQuery(query.Uri);
        //FormatCode = int.Parse(_uriQuery["itag"]);
        //MapFormat(FormatCode);
    }

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

    private void MapFormat(int formatCode)
    {
        Fps = FormatHelper.GetFps(formatCode);
        AudioBitrate = FormatHelper.GetAudioBitrate(formatCode);
        Resolution = FormatHelper.GetResolution(formatCode);
        Format = FormatHelper.GetVideoFormat(formatCode);
        AudioFormat = FormatHelper.GetAudioFormat(formatCode);
        AdaptiveKind = FormatHelper.GetAdaptiveKind(formatCode);
    }

    private void MapFormat(JsonNode formatNode)
    {
        FormatCode = formatNode["itag"].GetValue<int>();

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

        var audioQuality = formatNode["audioQuality"]?.GetValue<string>();
        AudioQuality = audioQuality;
    }
}