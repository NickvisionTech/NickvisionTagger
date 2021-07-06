using Microsoft.UI.Xaml.Controls;
using NickvisionTagger.Models;

namespace NickvisionTagger.Views.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        public SettingsDialog()
        {
            InitializeComponent();
        }

        private new async void Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            var configuration = await Configuration.LoadAsync();
            if (configuration.IsLightTheme)
            {
                BtnLight.IsChecked = true;
            }
            else
            {
                BtnDark.IsChecked = true;
            }
            TxtPreviousFolder.Text = configuration.PreviousMusicFolder;
            ChkIncludeSubfolders.IsChecked = configuration.IncludeSubfolders;
        }

        private new async void Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            var configuration = await Configuration.LoadAsync();
            configuration.IsLightTheme = (bool)BtnLight.IsChecked;
            configuration.IncludeSubfolders = (bool)ChkIncludeSubfolders.IsChecked;
            await configuration.SaveAsync();
        }
    }
}
