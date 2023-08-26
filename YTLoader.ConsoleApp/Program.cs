using YTLoader.ConsoleApp.YouTube;
using YTLoader.ConsoleApp.YouTube.Enums;

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

        var videos = youTubeVideo.FormatsInfo
            .Where(x => x.InfoFromResponse.AdaptiveKind == AdaptiveKind.Video && x.InfoFromResponse.Format == VideoFormat.Mp4)
            .OrderByDescending(x => x.InfoFromResponse.Resolution)
            .ToList();

        var first = youTubeVideo.FormatsInfo.FirstOrDefault(x => x.InfoFromUrl.Resolution == 720);
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

