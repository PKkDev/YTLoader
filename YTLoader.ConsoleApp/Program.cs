using VideoLibrary;
//using YTLoader.Core;
//using YTLoader.Core.Enums;


// https://github.com/omansak/libvideo 
// https://github.com/omansak/libvideo/blob/master/docs/README.md

try
{
    Task task = Task.Run(async () =>
    {
        //YouTubeClient youTubeClient = new();

        try
        {
            var youTube = YouTube.Default; // starting point for YouTube actions
            var uri = "https://www.youtube.com/watch?v=IssoDqVvc7A";

            //var videos = youTube.GetAllVideos(uri);

            var videoInfos = Client.For(YouTube.Default).GetAllVideosAsync(uri).GetAwaiter().GetResult();
            var resolutions = videoInfos.Where(j => j.AdaptiveKind == AdaptiveKind.Video).Select(j => j.Resolution);
            var bitRates = videoInfos.Where(j => j.AdaptiveKind == AdaptiveKind.Audio).Select(j => j.AudioBitrate);
            var unknownFormats = videoInfos.Where(j => j.AdaptiveKind == AdaptiveKind.None).Select(j => j.Resolution);

            //var video = youTube.GetVideo("https://www.youtube.com/watch?v=IssoDqVvc7A"); // gets a Video object with info about the video

            var video1 = videoInfos.Where(x => x.Resolution == 1440).ToList();
            var video = videoInfos.First(x => x.Resolution == 1440 && x.Format == VideoFormat.Mp4);
            File.WriteAllBytes(@"C:\" + video.FullName, video.GetBytes());

            //File.WriteAllBytes(@"C:\" + video.FullName, video.GetBytes());
            //File.WriteAllBytes(@"D:\Projects\YTLoader\" + video.FullName, video.GetBytes());


            //// var youTubeVideo = await youTubeClient.GetVideo("https://www.youtube.com/watch?v=LZvTEecjxos");
            //var youTubeVideo = await youTubeClient.GetVideo("https://www.youtube.com/watch?v=IssoDqVvc7A");

            //youTubeClient.ProgressChanged += (long? totalFileSize, long totalBytesDownloaded, double? progressPercentage) =>
            //{
            //    Console.WriteLine($"{progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})");
            //};

            //var a1 = youTubeVideo.FormatsInfo.Where(x => x.AudioFormat != AudioFormat.Unknown).ToList();
            //var a2 = youTubeVideo.FormatsInfo.Where(x => x.AudioQuality != null).ToList();

            //var forDownloads = youTubeVideo.FormatsInfo
            //    .Where(x
            //        => x.AdaptiveKind == AdaptiveKind.Video
            //        && x.Format == VideoFormat.Mp4
            //        && x.Codecs.Count > 1)
            //    .ToList();

            //var videos = youTubeVideo.FormatsInfo
            //    .Where(x
            //        => x.AdaptiveKind == AdaptiveKind.Video
            //        && x.Format == VideoFormat.Mp4
            //        && x.Resolution == 240)
            //    .OrderBy(x => x.Codecs.Count)
            //    .ToList();

            //var first = videos.First();
            //var bytes = await youTubeClient.GetBytes(first);
            //File.WriteAllBytes(@"D:\Projects\YTLoader\" + youTubeVideo.VideoName + first.FileExtension, bytes);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            //youTubeClient.Dispose();
        }

    });
    task.Wait();
}
catch (Exception e)
{
    Console.WriteLine(e);
}

