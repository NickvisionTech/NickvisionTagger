using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NickvisionTagger.Views.Dialogs
{
    public sealed partial class ComboBoxDialog : ContentDialog
    {
        public ComboBoxDialog(XamlRoot xamlRoot, string title, string description, string comboBoxHeader, List<string> options)
        {
            InitializeComponent();
            Title = title;
            XamlRoot = xamlRoot;
            LblDescription.Text = description;
            CmbBox.Header = comboBoxHeader;
            foreach(var option in options)
            {
                CmbBox.Items.Add(option);
            }
        }

        public new async Task<(ContentDialogResult Result, string SelectedOption)> ShowAsync()
        {
            var result = await base.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                return (result, CmbBox.SelectedItem as string);
            }
            return (result, null);
        }
    }
}
