using Microsoft.UI.Xaml;

namespace YTLoader.WinApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel();
        }
    }
}
