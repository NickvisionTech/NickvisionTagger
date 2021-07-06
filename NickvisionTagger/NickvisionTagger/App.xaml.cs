using Microsoft.UI.Xaml;
using NickvisionTagger.Models;
using NickvisionTagger.Views;

namespace NickvisionTagger
{
    public partial class App : Application
    {
        private Window _window;

        public App()
        {
            var configuration = Configuration.LoadAsync().GetAwaiter().GetResult();
            InitializeComponent();
            RequestedTheme = configuration.IsLightTheme ? ApplicationTheme.Light : ApplicationTheme.Dark;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
            PInvoke.User32.ShowWindow(PInvoke.User32.GetActiveWindow(), PInvoke.User32.WindowShowStyle.SW_MAXIMIZE);
        }
    }
}
