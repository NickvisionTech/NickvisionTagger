using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace NickvisionTagger.Views.Dialogs
{
    public sealed partial class ProgressDialog : ContentDialog
    {
        private Func<Task> _action;

        public ProgressDialog(XamlRoot xamlRoot, string description, Func<Task> action)
        {
            InitializeComponent();
            XamlRoot = xamlRoot;
            LblDescription.Text = description;
            _action = action;
        }

        private new async void Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            await _action();
            Hide();
        }
    }
}
