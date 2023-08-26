using YTLoader.ConsoleApp.YouTube;

try
{
    Task task = Task.Run(async () =>
    {
        YouTubeClient youTubeClient = new YouTubeClient();
        var video = await youTubeClient.GetVideo("https://www.youtube.com/watch?v=LZvTEecjxos");

        var first = video.FormatsInfo.FirstOrDefault(x => x.InfoFromUrl.Resolution == 720);
        var b = await youTubeClient.GetBytes(first);

        File.WriteAllBytes(@"D:\Projects\YTLoader\" + video.VideoName + first.FileExtension, b);

    });
    task.Wait();
}
catch (Exception e)
{
    Console.WriteLine(e);
}