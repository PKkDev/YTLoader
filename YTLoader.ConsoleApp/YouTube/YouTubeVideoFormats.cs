namespace YTLoader.ConsoleApp.YouTube;

public static class YouTubeVideoFormats
{
    public static int GetFps(int formatCode)
    {
        return formatCode switch
        {
            571 or 402 or 401 or 400 or 399 or 398 or 337 or
            336 or 335 or 334 or 333 or 332 or 331 or 330 or
            272 or 315 or 308 or 303 or 302 or 305 or 304 or
            299 or 298
                => 60,
            18 or 22 or 37 or 43 or 59 or 397 or 396 or 395 or
            394 or 313 or 271 or 248 or 247 or 244 or 243 or
            242 or 278 or 138 or 266 or 264 or 137 or 136 or
            135 or 134 or 133 or 160
                => 30,
            _ => -1,
        };
    }

    public static int GetAudioBitrate(int formatCode)
    {
        return formatCode switch
        {
            139 or 249 or 250 => 48,
            18 => 96,
            37 or 43 or 59 or 140 or 171 or 251 => 128,
            22 or 256 => 192,
            141 or 172 or 327 => 256,
            258 => 384,
            338 => 480,
            _ => -1,
        };
    }

    public static int GetResolution(int formatCode)
    {
        return formatCode switch
        {
            394 or 330 or 278 or 160
                => 144,
            395 or 331 or 242 or 133
                => 240,
            18 or 43 or 396 or 332 or 243 or 134
                => 360,
            59 or 397 or 333 or 244 or 135
                => 480,
            22 or 398 or 334 or 302 or 247 or 298 or 136
                => 720,
            37 or 399 or 335 or 303 or 248 or 299 or 137
                => 1080,
            400 or 336 or 308 or 271 or 304 or 264
                => 1440,
            401 or 337 or 315 or 313 or 305 or 266
                => 2160,
            138 or 272 or 402 or 571
                => 4320,
            _ => -1
        };
    }

    public static VideoFormat GetVideoFormat(int formatCode)
    {
        return formatCode switch
        {
            18 or 22 or 37 or 59 or 133 or 134 or 135 or 136 or
            137 or 138 or 160 or 264 or 266 or 298 or 299 or
            304 or 305 or 394 or 395 or 396 or 397 or 398 or
            399 or 400 or 401 or 402 or 571
                => VideoFormat.Mp4,
            43 or 242 or 243 or 244 or 247 or 248 or 271 or
            272 or 302 or 303 or 308 or 313 or 315 or 330 or 331 or
            332 or 333 or 334 or 335 or 336 or 337
                => VideoFormat.WebM,
            _ => VideoFormat.Unknown,
        };
    }

    public static AudioFormat GetAudioFormat(int formatCode)
    {
        return formatCode switch
        {
            18 or 22 or 37 or 59 or 139 or
            140 or 141 or 256 or 258 or 327
                => AudioFormat.Aac,
            171 or 172
                => AudioFormat.Vorbis,
            43 or 249 or 250 or 251 or 338
                => AudioFormat.Opus,
            _ => AudioFormat.Unknown,
        };
    }

    public static AdaptiveKind GetAdaptiveKind(int formatCode)
    {
        return formatCode switch
        {
            18 or 22 or 37 or 43 or 59 or 133 or 134 or 135 or 136 or
            137 or 138 or 160 or 242 or 243 or 244 or 247 or 248 or
            264 or 266 or 271 or 272 or 298 or 299 or 302 or 303 or
            304 or 305 or 308 or 313 or 315 or 330 or 331 or 332 or
            333 or 334 or 335 or 336 or 337 or 394 or 395 or 396 or
            397 or 398 or 399 or 400 or 401 or 402 or 571
                => AdaptiveKind.Video,
            139 or 140 or 141 or 171 or 172 or 249 or 250 or
            251 or 256 or 258 or 327 or 338
                => AdaptiveKind.Audio,
            _
                => AdaptiveKind.None
        };
    }
}

public enum VideoFormat
{
    Mp4,
    WebM,
    Unknown
}

public enum AudioFormat
{
    Aac = 0,
    Vorbis = 1,
    Opus = 2,
    Unknown = 3
}

public enum AdaptiveKind
{
    None,
    Audio,
    Video
}
