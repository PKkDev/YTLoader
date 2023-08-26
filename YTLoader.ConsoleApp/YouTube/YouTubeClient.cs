using System.Net;
using System.Text.RegularExpressions;
using System.Text.Json.Nodes;

namespace YTLoader.ConsoleApp.YouTube
{
    public class YouTubeClient
    {
        private HttpClient _client { get; set; }

        public YouTubeClient()
        {
            _client = CreateClient();
        }

        public async Task<VideoInfo> GetVideo(string url)
        {
            var pageContent = await _client.GetStringAsync(url);
            File.WriteAllText("D:/Projects/YTLoader/YTLoader.ConsoleApp/response.html", pageContent);

            var playerData = ParsePlayerJson(pageContent);
            File.WriteAllText("D:/Projects/YTLoader/YTLoader.ConsoleApp/InitialPlayerResponse.json", playerData);

            JsonNode? playerDataJson = JsonNode
                .Parse(playerData)
                ?? throw new Exception("Error on parsing playerResponse");

            #region check error

            var status = playerDataJson["playabilityStatus"]?["status"]?.GetValue<string>();
            if (string.Equals(status, "error", StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Video has unavailable stream.");

            var errorReason = playerDataJson["playabilityStatus"]?["reason"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(errorReason))
                throw new Exception($"Error caused by Youtube.({errorReason})");

            #endregion check error

            #region check live

            var isLive = playerDataJson["videoDetails"]?["isLive"]?.GetValue<bool>() == true;
            var isLiveContent = playerDataJson["videoDetails"]?["isLiveContent"]?.GetValue<bool>() == true;
            if (isLive || isLiveContent)
                throw new Exception($"This is live stream so unavailable stream");

            #endregion check live

            #region video details

            var videoDetails = playerDataJson["videoDetails"];

            int? lengthSeconds = null;
            var lengthSecondsNode = videoDetails?["lengthSeconds"];
            if (lengthSecondsNode != null)
                lengthSeconds = Convert.ToInt32(lengthSecondsNode.GetValue<string>());

            var title = videoDetails?["title"]?.GetValue<string>();

            var author = videoDetails?["author"]?.GetValue<string>();

            var videoId = videoDetails?["videoId"]?.GetValue<string>();

            List<string> keywords = new();
            var keywordNodes = videoDetails?["keywords"]?.AsArray();
            foreach (var keywordNode in keywordNodes)
            {
                if (keywordNode != null)
                    keywords.Add(keywordNode.GetValue<string>());
            }

            var videoInfo = new VideoInfo(title, lengthSeconds, author, videoId, keywords);

            #endregion video details

            List<JsonNode> streamObjects = new();
            // Extract Muxed streams
            var streamFormat = playerDataJson["streamingData"]["formats"];
            if (streamFormat != null)
            {
                streamObjects.AddRange(streamFormat.AsArray());
            }
            // Extract AdaptiveFormat streams
            var streamAdaptiveFormats = playerDataJson["streamingData"]["adaptiveFormats"];
            if (streamAdaptiveFormats != null)
            {
                streamObjects.AddRange(streamAdaptiveFormats.AsArray());
            }

            foreach (var item in streamObjects)
            {
                var urlValue = item["url"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(urlValue))
                {
                    var query = new UnscrambledQuery(urlValue, false);
                    videoInfo.YouTubeVideos.Add(new YouTubeVideo(query, playerData, item));
                    continue;
                }

                var cipherValue = ((item["cipher"] ?? item["signatureCipher"]) ?? string.Empty).GetValue<string>();
                if (!string.IsNullOrEmpty(cipherValue))
                {
                    //yield return new YouTubeVideo(videoInfo, Unscramble(cipherValue), playerData);
                    var query = new UnscrambledQuery(cipherValue, false);
                    videoInfo.YouTubeVideos.Add(new YouTubeVideo(query, playerData, item));
                }
            }

            return videoInfo;
        }

        public async Task<byte[]> GetBytes(YouTubeVideo video)
        {
            var b = await _client.GetByteArrayAsync(video.VideoUrl);
            return b;
        }

        private HttpClient CreateClient()
        {
            // Cookie
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("https://youtube.com/"), new Cookie("CONSENT", "YES+cb", "/", ".youtube.com"));
            // Handler
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer
            };
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36 Edg/89.0.774.76"
            );

            return httpClient;
        }

        private string ParsePlayerJson(string source)
        {
            string playerResponseMap = null;
            string ytInitialPlayerPattern = @"\s*var\s*ytInitialPlayerResponse\s*=\s*(\{\""responseContext\"".*\});";
            string ytWindowInitialPlayerResponse = @"\[\""ytInitialPlayerResponse\""\]\s*=\s*(\{.*\});";
            string ytPlayerPattern = @"ytplayer\.config\s*=\s*(\{\"".*\""\}\});";

            Match match;
            //if ((match = Regex.Match(source, ytPlayerPattern)).Success && Json.TryGetKey("player_response", match.Groups[1].Value, out string json))
            //{
            //    playerResponseMap = Regex.Unescape(json);
            //}

            if (string.IsNullOrWhiteSpace(playerResponseMap) && (match = Regex.Match(source, ytInitialPlayerPattern)).Success)
            {
                playerResponseMap = match.Groups[1].Value;
            }

            if (string.IsNullOrWhiteSpace(playerResponseMap) && (match = Regex.Match(source, ytWindowInitialPlayerResponse)).Success)
            {
                playerResponseMap = match.Groups[1].Value;
            }

            if (string.IsNullOrWhiteSpace(playerResponseMap))
            {
                throw new Exception("Player json has no found.");
            }

            return playerResponseMap.Replace(@"\u0026", "&").Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\\&", "\\\\&");
        }
    }
}
