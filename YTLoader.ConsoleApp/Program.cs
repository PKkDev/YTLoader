using YTLoader.Core;
using YTLoader.Core.Enums;

try
{
    Task task = Task.Run(async () =>
    {
        YouTubeClient youTubeClient = new();

        try
        {
            // var youTubeVideo = await youTubeClient.GetVideo("https://www.youtube.com/watch?v=LZvTEecjxos");
            var youTubeVideo = await youTubeClient.GetVideo("https://www.youtube.com/watch?v=FbIRBPhdbYg");

            youTubeClient.ProgressChanged += (long? totalFileSize, long totalBytesDownloaded, double? progressPercentage) =>
            {
                Console.WriteLine($"{progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})");
            };

            var a1 = youTubeVideo.FormatsInfo.Where(x => x.AudioFormat != AudioFormat.Unknown).ToList();
            var a2 = youTubeVideo.FormatsInfo.Where(x => x.AudioQuality != null).ToList();

            var forDownloads = youTubeVideo.FormatsInfo
                .Where(x
                    => x.AdaptiveKind == AdaptiveKind.Video
                    && x.Format == VideoFormat.Mp4
                    && x.Codecs.Count > 1)
                .ToList();

            var videos = youTubeVideo.FormatsInfo
                .Where(x
                    => x.AdaptiveKind == AdaptiveKind.Video
                    && x.Format == VideoFormat.Mp4
                    && x.Resolution == 240)
                .OrderBy(x => x.Codecs.Count)
                .ToList();

            var first = videos.First();
            var bytes = await youTubeClient.GetBytes(first);
            File.WriteAllBytes(@"D:\Projects\YTLoader\" + youTubeVideo.VideoName + first.FileExtension, bytes);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            youTubeClient.Dispose();
        }

    });
    task.Wait();
}
catch (Exception e)
{
    Console.WriteLine(e);
}

