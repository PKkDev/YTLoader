using System.Collections.ObjectModel;
using YTLoader.Core.Types;
using YTLoader.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

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

        public RelayCommand OnAnalyzVideo { get; set; }

        private readonly YouTubeClient youTubeClient;

        private VideoInfo videoInfo;

        public ObservableCollection<FormatInfo> AllFormats { get; set; }
        private FormatInfo _selectedFormat;
        public FormatInfo SelectedFormat
        {
            get { return _selectedFormat; }
            set
            {
                SetProperty(ref _selectedFormat, value);
            }
        }

        public MainWindowViewModel()
        {
            youTubeClient = new();
            AllFormats = new();
            InputVideoUrl = "https://www.youtube.com/watch?v=LZvTEecjxos";

            OnAnalyzVideo = new RelayCommand(async () => await AnalyzVideo());
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
    }
}
