using YTLoader.Core;
using YTLoader.Core.Enums;

try
{
    Task task = Task.Run(async () =>
    {
        YouTubeClient youTubeClient = new();
        var youTubeVideo = await youTubeClient.GetVideo("https://www.youtube.com/watch?v=LZvTEecjxos");

        youTubeClient.ProgressChanged += (long? totalFileSize, long totalBytesDownloaded, double? progressPercentage) =>
        {
            Console.WriteLine($"{progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})");
        };

        var a1 = youTubeVideo.FormatsInfo.Where(x => x.InfoFromUrl.AudioFormat != AudioFormat.Unknown).ToList();
        var a2 = youTubeVideo.FormatsInfo.Where(x => x.InfoFromResponse.AudioQuality != null).ToList();

        var forDownloads = youTubeVideo.FormatsInfo
            .Where(x
                => x.InfoFromResponse.AdaptiveKind == AdaptiveKind.Video
                && x.InfoFromResponse.Format == VideoFormat.Mp4
                && x.InfoFromResponse.Codecs.Count > 1)
            .ToList();

        var videos = youTubeVideo.FormatsInfo
            .Where(x
                => x.InfoFromResponse.AdaptiveKind == AdaptiveKind.Video
                && x.InfoFromResponse.Format == VideoFormat.Mp4
                && x.InfoFromResponse.Resolution == 720)
            .OrderBy(x => x.InfoFromResponse.Codecs.Count)
            .ToList();

        var first = videos.First();
        var b = await youTubeClient.GetBytes(first);

        File.WriteAllBytes(@"D:\Projects\YTLoader\" + youTubeVideo.VideoName + first.FileExtension, b);

        youTubeClient.Dispose();

    });
    task.Wait();
}
catch (Exception e)
{
    Console.WriteLine(e);
}

