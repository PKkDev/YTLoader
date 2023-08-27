using System.Collections.ObjectModel;
using YTLoader.Core.Types;
using YTLoader.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Text;
using Windows.System;
using Windows.Storage;
using System;
using System.IO;

namespace YTLoader.WinApp
{
    public class MainWindowViewModel : ObservableRecipient
    {
        public string InputVideoUrl { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        private string _lengthSeconds;
        public string LengthSeconds
        {
            get => _lengthSeconds;
            set => SetProperty(ref _lengthSeconds, value);
        }
        private string _author;
        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }
        private string _videoId;
        public string VideoId
        {
            get => _videoId;
            set => SetProperty(ref _videoId, value);
        }
        private string _keywords;
        public string Keywords
        {
            get => _keywords;
            set => SetProperty(ref _keywords, value);
        }

        private StorageFolder _currentFolder;
        public StorageFolder CurrentFolder
        {
            get => _currentFolder;
            set => SetProperty(ref _currentFolder, value);
        }

        public RelayCommand OnAnalyzVideo { get; set; }
        public RelayCommand OnDownloadVideo { get; set; }
        public RelayCommand OnOpenCurrentFolderCmd { get; set; }

        private readonly YouTubeClient youTubeClient;

        private VideoInfo videoInfo;

        private string _formatInfoTxt;
        public string FormatInfoTxt
        {
            get => _formatInfoTxt;
            set => SetProperty(ref _formatInfoTxt, value);
        }

        private string _downloadMsg;
        public string DownloadMsg
        {
            get => _downloadMsg;
            set => SetProperty(ref _downloadMsg, value);
        }

        public ObservableCollection<FormatInfo> AllFormats { get; set; }
        private FormatInfo _selectedFormat;
        public FormatInfo SelectedFormat
        {
            get { return _selectedFormat; }
            set
            {
                this.OnViewDetailForFormat(value);
                SetProperty(ref _selectedFormat, value);
            }
        }

        public MainWindowViewModel()
        {
            youTubeClient = new();
            youTubeClient.ProgressChanged += (long? totalFileSize, long totalBytesDownloaded, double? progressPercentage) =>
            {
                DownloadMsg = $"{progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})";
            };

            AllFormats = new();
            CurrentFolder = ApplicationData.Current.LocalCacheFolder;
            //InputVideoUrl = "https://www.youtube.com/watch?v=LZvTEecjxos";
            InputVideoUrl = "https://www.youtube.com/watch?v=FbIRBPhdbYg";

            OnAnalyzVideo = new RelayCommand(async () => await AnalyzVideo());
            OnDownloadVideo = new RelayCommand(async () => await DownloadVideo());
            OnOpenCurrentFolderCmd = new RelayCommand(async () =>
            {
                if (CurrentFolder != null)
                    await Launcher.LaunchFolderAsync(CurrentFolder);
            });
        }

        private void OnViewDetailForFormat(FormatInfo format)
        {
            if (SelectedFormat == null || SelectedFormat.FormatCode != format.FormatCode)
            {
                StringBuilder sb = new();
                sb.AppendLine($"FormatCode: {format.FormatCode}");
                sb.AppendLine($"Fps: {format.Fps}");
                sb.AppendLine($"AudioBitrate: {format.AudioBitrate}");
                sb.AppendLine($"Resolution: {format.Resolution}");
                sb.AppendLine($"QualityLabel: {format.QualityLabel}");
                sb.AppendLine($"MimeType: {format.MimeType}");
                sb.AppendLine($"Codecs: {string.Join(',', format.Codecs)}");
                sb.AppendLine($"Format: {format.Format}");
                sb.AppendLine($"AudioFormat: {format.AudioFormat}");
                sb.AppendLine($"AdaptiveKind: {format.AdaptiveKind}");
                sb.AppendLine($"AudioQuality: {format.AudioQuality}");
                FormatInfoTxt = sb.ToString();
            }
        }

        private async Task AnalyzVideo()
        {
            videoInfo = await youTubeClient.GetVideo(InputVideoUrl);

            Title = videoInfo.Title;
            LengthSeconds = videoInfo.LengthSeconds.ToString();
            Author = videoInfo.Author;
            VideoId = videoInfo.VideoId;
            Keywords = string.Join(',', videoInfo.Keywords);

            AllFormats.Clear();
            foreach (var format in videoInfo.FormatsInfo)
            {
                AllFormats.Add(format);
            };
        }

        private async Task DownloadVideo()
        {
            if (SelectedFormat == null || videoInfo == null)
            {
                return;
            }

            var bytes = await youTubeClient.GetBytes(SelectedFormat);

            StorageFile file = await CurrentFolder.CreateFileAsync($"{videoInfo.VideoName}{SelectedFormat.FileExtension}", CreationCollisionOption.GenerateUniqueName);
            using Stream stream = await file.OpenStreamForWriteAsync();
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
