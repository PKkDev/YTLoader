using System.Net;
using System.Text.RegularExpressions;
using System.Text.Json.Nodes;
using YTLoader.Core.Types;
using System;

namespace YTLoader.Core;

public class YouTubeClient : IDisposable
{
    public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

    public event ProgressChangedHandler ProgressChanged;

    private HttpClient _client { get; set; }

    private string _signatureKey { get; set; } = "signature";

    private VideoInfo _videoInfo { get; set; }

    public YouTubeClient()
    {
        _client = CreateClient();
    }

    public async Task<VideoInfo> GetVideo(string url)
    {
        var pageContent = await _client.GetStringAsync(url);
        //File.WriteAllText("D:/Projects/YTLoader/YTLoader.ConsoleApp/response.html", pageContent);

        var playerData = ParsePlayerJson(pageContent);
        //File.WriteAllText("D:/Projects/YTLoader/YTLoader.ConsoleApp/InitialPlayerResponse.json", playerData);

        string? jsPlayer = null;

        var jsUrlPlayer1 = Regex.Match(pageContent, @"""jsUrl"":\s*""(\S*\.js)"",");
        if (jsUrlPlayer1.Groups.Count > 1)
            jsPlayer = jsUrlPlayer1.Groups[1].Value;

        if (jsPlayer == null)
        {
            var jsUrlPlayer2 = Regex.Match(pageContent, @"""PLAYER_JS_URL"":\s*""(\S*\.js)"",");
            if (jsUrlPlayer2.Groups.Count > 1)
                jsPlayer = jsUrlPlayer2.Groups[1].Value;
        }

        if (jsPlayer == null)
            throw new Exception("JsPlayer not found");

        if (jsPlayer.StartsWith("/yts") || jsPlayer.StartsWith("/s"))
            jsPlayer = $"https://www.youtube.com{jsPlayer}";

        if (!jsPlayer.StartsWith("http"))
            jsPlayer = $"https:{jsPlayer}";

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

        var videoInfo = new VideoInfo(title, lengthSeconds, author, videoId, keywords, playerData, jsPlayer);

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
                videoInfo.FormatsInfo.Add(new FormatInfo(query, item));
                continue;
            }

            var cipherValue = ((item["cipher"] ?? item["signatureCipher"]) ?? string.Empty).GetValue<string>();
            if (!string.IsNullOrEmpty(cipherValue))
            {
                var query = Unscramble(cipherValue);
                videoInfo.FormatsInfo.Add(new FormatInfo(query, item));
            }
        }

        _videoInfo = videoInfo;

        return videoInfo;
    }

    private UnscrambledQuery Unscramble(string queryString)
    {
        queryString = queryString.Replace(@"\u0026", "&");
        var query = new FormatInfoQuery(queryString);
        string uri = query["url"];

        query.TryGetValue("sp", out var signatureKey);
        _signatureKey = signatureKey;

        bool encrypted = false;
        string signature;

        if (query.TryGetValue("s", out signature))
        {
            encrypted = true;
            uri += GetSignatureAndHost(_signatureKey, signature, query);
        }
        else
        {
            if (query.TryGetValue("sig", out signature))
            {
                uri += GetSignatureAndHost(_signatureKey, signature, query);
            }
        }

        uri = WebUtility.UrlDecode(WebUtility.UrlDecode(uri));

        var uriQuery = new FormatInfoQuery(uri);

        if (!uriQuery.ContainsKey("ratebypass"))
            uri += "&ratebypass=yes";

        return new UnscrambledQuery(uri, encrypted);
    }

    private string GetSignatureAndHost(string key, string signature, FormatInfoQuery query)
    {
        string result = $"&{key}={signature}";

        string host;

        if (query.TryGetValue("fallback_host", out host))
            result += "&fallback_host=" + host;

        return result;
    }

    public async Task<byte[]> GetBytes(FormatInfo video)
    {
        var videoUrl = video.VideoUrl;

        if (video.IsEncrypted)
        {
            videoUrl = await DecryptAsync(video.VideoUrl);
        }

        MemoryStream stream = new();

        using (var response = await _client.GetAsync(videoUrl, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;

            using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                var totalBytesRead = 0L;
                var buffer = new byte[81920];
                var isMoreToRead = true;

                do
                {
                    var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        isMoreToRead = false;
                        TriggerProgressChanged(totalBytes, totalBytesRead);
                        continue;
                    }
                    await stream.WriteAsync(buffer, 0, bytesRead);

                    totalBytesRead += bytesRead;

                    TriggerProgressChanged(totalBytes, totalBytesRead);
                }
                while (isMoreToRead);
            }
        }

        return stream.ToArray();
    }

    private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
    {
        if (ProgressChanged == null)
            return;

        double? progressPercentage = null;
        if (totalDownloadSize.HasValue)
            progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

        Console.WriteLine($"{progressPercentage}% ({totalBytesRead}/{totalDownloadSize})");

        ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
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
            CookieContainer = cookieContainer,

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

    public void Dispose()
    {
        _client?.Dispose();
    }

    #region DecryptAsync

    private async Task<string> DecryptAsync(string url)
    {
        var jsPlayer = await _client.GetStringAsync(_videoInfo.JsPlayerUrl);

        //File.WriteAllText("D:/Projects/YTLoader/YTLoader.ConsoleApp/jsplayer.js", jsPlayer);

        var query = new FormatInfoQuery(url);

        string signature;
        if (!query.TryGetValue(_signatureKey, out signature))
            return url;

        if (string.IsNullOrWhiteSpace(signature))
            throw new Exception("Signature not found.");

        #region

        var decipherFuncName = Regex.Match(jsPlayer, @"(\w+)=function\(\w+\){(\w+)=\2\.split\(\x22{2}\);.*?return\s+\2\.join\(\x22{2}\)}");
        var functionLines = decipherFuncName.Success ? decipherFuncName.Groups[0].Value.Split(';') : null;

        #endregion

        var decipherDefinitionName = Regex.Match(string.Join(";", functionLines), "([\\$_\\w]+).\\w+\\(\\w+,\\d+\\);").Groups[1].Value;
        if (string.IsNullOrEmpty(decipherDefinitionName))
        {
            throw new Exception("Could not find signature decipher definition name. Please report this issue to us.");
        }

        var decipherDefinitionBody = Regex.Match(jsPlayer, $@"var\s+{Regex.Escape(decipherDefinitionName)}=\{{(\w+:function\(\w+(,\w+)?\)\{{(.*?)\}}),?\}};", RegexOptions.Singleline).Groups[0].Value;
        if (string.IsNullOrEmpty(decipherDefinitionBody))
        {
            throw new Exception("Could not find signature decipher definition body. Please report this issue to us.");
        }


        //query[_signatureKey] = DecryptSignature(jsPlayer, _signatureKey);


        return string.Empty;
    }

    #endregion DecryptAsync
}