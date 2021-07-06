using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionTagger.Extensions;
using NickvisionTagger.Helpers;
using NickvisionTagger.Models;
using NickvisionTagger.Models.Update;
using NickvisionTagger.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.System;
using WinRT;

namespace NickvisionTagger.Views
{
    public sealed partial class MainWindow : Window
    {
        private bool _opened;
        private MusicFolder _musicFolder;

        public MainWindow()
        {
            InitializeComponent();
            _opened = false;
            _musicFolder = new MusicFolder();
            Title = "Nickvision Tagger";
        }

        private new async void Activated(object sender, WindowActivatedEventArgs args)
        {
            if (!_opened)
            {
                var configuration = await Configuration.LoadAsync();
                if(Directory.Exists(configuration.PreviousMusicFolder))
                {
                    _musicFolder.Path = configuration.PreviousMusicFolder;
                    LblStatus.Text = _musicFolder.Path;
                }
                _musicFolder.IncludeSubfolders = configuration.IncludeSubfolders;
                ReloadMusicFolder(null, null);
                _opened = true;
            }
        }

        private async void OpenMusicFolder(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderPicker();
            folderDialog.FileTypeFilter.Add("*");
            if (Current == null)
            {
                var initializeWithWindowWrapper = folderDialog.As<IInitializeWithWindow>();
                var hwnd = PInvoke.User32.GetActiveWindow();
                initializeWithWindowWrapper.Initialize(hwnd);
            }
            var folder = await folderDialog.PickSingleFolderAsync();
            if (folder != null)
            {
                var configuration = await Configuration.LoadAsync();
                configuration.PreviousMusicFolder = folder.Path;
                _musicFolder.Path = folder.Path;
                LblStatus.Text = folder.Path;
                await configuration.SaveAsync();
                ReloadMusicFolder(null, null);
            }
        }

        private async void CloseMusicFolder(object sender, RoutedEventArgs e)
        {
            var configuration = await Configuration.LoadAsync();
            configuration.PreviousMusicFolder = "";
            _musicFolder.Path = "";
            LblStatus.Text = "No Folder Open";
            await configuration.SaveAsync();
            ReloadMusicFolder(null, null);
        }

        private async void ReloadMusicFolder(object sender, RoutedEventArgs e)
        {
            if(Menu.XamlRoot != null)
            {
                await new ProgressDialog(Menu.XamlRoot, "Loading music files...", async () => await _musicFolder.RescanFilesAsync()).ShowAsync();
            }
            else
            {
                await ShowInfoBarAsync("Please Wait", "Loading music files...", InfoBarSeverity.Warning);
                await _musicFolder.RescanFilesAsync();
                InfoBar.IsOpen = false;
            }
            ListMusicFiles.ItemsSource = null;
            ListMusicFiles.ItemsSource = _musicFolder.Files;
        }

        private async void SaveTag(object sender, RoutedEventArgs e)
        {
            var selectedItems = ListMusicFiles.SelectedItems;
            if (selectedItems.Count != 0)
            {
                var filename = TxtFilename.Text;
                var isFilenameReadOnly = TxtFilename.IsReadOnly;
                var title = TxtTitle.Text;
                var artist = TxtArtist.Text;
                var album = TxtAlbum.Text;
                var year = TxtYear.Text;
                var track = TxtTrack.Text;
                var albumArtist = TxtAlbumArtist.Text;
                var genre = TxtGenre.Text;
                var comment = TxtComment.Text;
                await new ProgressDialog(Menu.XamlRoot, "Saving tags...", async () =>
                {
                    await Task.Run(() =>
                    {
                        foreach (var item in selectedItems)
                        {
                            var musicFile = item as MusicFile;
                            if (string.IsNullOrEmpty(filename))
                            {
                                continue;
                            }
                            if(musicFile.Filename != filename && !isFilenameReadOnly)
                            {
                                musicFile.Filename = filename;
                            }
                            if(title != "<keep>")
                            {
                                musicFile.Title = title;
                            }
                            if (artist != "<keep>")
                            {
                                musicFile.Artist = artist;
                            }
                            if (album != "<keep>")
                            {
                                musicFile.Album = album;
                            }
                            if (year != "<keep>")
                            {
                                try
                                {
                                    musicFile.Year = uint.Parse(year);
                                }
                                catch { }
                            }
                            if (track != "<keep>")
                            {
                                try
                                {
                                    musicFile.Track = uint.Parse(track);
                                }
                                catch { }
                            }
                            if (albumArtist != "<keep>")
                            {
                                musicFile.AlbumArtist = albumArtist;
                            }
                            if (genre != "<keep>")
                            {
                                musicFile.Genre = genre;
                            }
                            if (comment != "<keep>")
                            {
                                musicFile.Comment = comment;
                            }
                            musicFile.Save();
                        }
                    });
                }).ShowAsync();
                ReloadMusicFolder(null, null);
            }
        }

        private async void RemoveTag(object sender, RoutedEventArgs e)
        {
            var selectedItems = ListMusicFiles.SelectedItems;
            if(selectedItems.Count != 0)
            {
                var areYouSureDialog = new ContentDialog()
                {
                    Title = "Remove Tags?",
                    Content = $"Are you sure you want to remove {selectedItems.Count} tag(s)?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = Menu.XamlRoot
                };
                var result = await areYouSureDialog.ShowAsync();
                if(result == ContentDialogResult.Primary)
                {
                    await new ProgressDialog(Menu.XamlRoot, "Removing tags...", async () =>
                    {
                        await Task.Run(() =>
                        {
                            foreach (var item in selectedItems)
                            {
                                var musicFile = item as MusicFile;
                                musicFile.RemoveTag();
                            }
                        });
                    }).ShowAsync();
                    ReloadMusicFolder(null, null);
                }
            }
        }

        private void Exit(object sender, RoutedEventArgs e) => Close();

        private async void Settings(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new SettingsDialog()
            {
                XamlRoot = Menu.XamlRoot
            };
            await settingsDialog.ShowAsync();
            var configuration = await Configuration.LoadAsync();
            _musicFolder.IncludeSubfolders = configuration.IncludeSubfolders;
            ReloadMusicFolder(null, null);
        }

        private async void FilenameToTag(object sender, RoutedEventArgs e)
        {
            var selectedItems = ListMusicFiles.SelectedItems;
            if(selectedItems.Count != 0)
            {
                var formatStrings = new List<string>() { "%artist%- %title%", "%title%- %artist%", "%track% %title%", "%title%" };
                var result = await new ComboBoxDialog(Menu.XamlRoot, "Filename To Tag", "Please select a format string", "Format String", formatStrings).ShowAsync();
                if(result.Result == ContentDialogResult.Primary)
                {
                    await new ProgressDialog(Menu.XamlRoot, "Converting filenames to tags...", async () =>
                    {
                        await Task.Run(() =>
                        {
                            foreach (var item in selectedItems)
                            {
                                var musicFile = item as MusicFile;
                                try
                                {
                                    musicFile.FilenameToTag(result.SelectedOption);
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        });
                    }).ShowAsync();
                    ReloadMusicFolder(null, null);
                }
            }
        }

        private async void TagToFilename(object sender, RoutedEventArgs e)
        {
            var selectedItems = ListMusicFiles.SelectedItems;
            if (selectedItems.Count != 0)
            {
                var formatStrings = new List<string>() { "%artist%- %title%", "%title%- %artist%", "%track% %title%", "%title%" };
                var result = await new ComboBoxDialog(Menu.XamlRoot, "Tag To Filename", "Please select a format string", "Format String", formatStrings).ShowAsync();
                if (result.Result == ContentDialogResult.Primary)
                {
                    await new ProgressDialog(Menu.XamlRoot, "Converting tags to filenames...", async () =>
                    {
                        await Task.Run(() =>
                        {
                            foreach (var item in selectedItems)
                            {
                                var musicFile = item as MusicFile;
                                try
                                {
                                    musicFile.TagToFilename(result.SelectedOption);
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        });
                    }).ShowAsync();
                    ReloadMusicFolder(null, null);
                }
            }
        }

        private async void CheckForUpdates(object sender, RoutedEventArgs e)
        {
            var updater = new Updater("https://raw.githubusercontent.com/NickvisionTech/NickvisionTagger/main/NickvisionTagger/NickvisionTagger/updateConfig.json", new Version("2021.7.0"));
            await new ProgressDialog(Menu.XamlRoot, "Checking for updates...", async () => await updater.CheckForUpdatesAsync()).ShowAsync();
            if (updater.UpdateAvaliable)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Update Available",
                    Content = $"=== V{updater.LatestVersion} Changelog ===\n{updater.Changelog}\n\nPlease visit the Microsoft Store to get the latest version of Nickvision Tagger.",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = Menu.XamlRoot
                };
                await dialog.ShowAsync();
            }
            else
            {
                await ShowInfoBarAsync("No Update Available", "No update is available at this time.", InfoBarSeverity.Error, 2500);
            }
        }

        private async void GitHubRepo(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri("https://github.com/NickvisionTech/NickvisionTagger/"));

        private async void ReportABug(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri("https://github.com/NickvisionTech/NickvisionTagger/issues/new"));

        private async void BuyMeACoffee(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri("https://www.buymeacoffee.com/nlogozzo"));

        private async void Changelog(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog()
            {
                Title = "What's New?",
                Content = "- Initial Release",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = Menu.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void About(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog()
            {
                Title = "About",
                Content = "Nickvision Tagger Version 2021.7.0\nA music metadata (tag) editor\n\nBuilt With: NickvisionApps Generation 5 (C# and WinUI 3)\n\nDeveloper: Nicholas Logozzo",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = Menu.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async Task ShowInfoBarAsync(string title, string message, InfoBarSeverity severity, int waitTimeMilliseconds = 0, bool isClosable = false)
        {
            InfoBar.Title = title;
            InfoBar.Message = message;
            InfoBar.Severity = severity;
            InfoBar.IsClosable = isClosable;
            InfoBar.IsOpen = true;
            if (waitTimeMilliseconds > 0)
            {
                await Task.Delay(waitTimeMilliseconds);
                InfoBar.IsOpen = false;
            }
            else
            {
                await Task.Delay(10);
            }
        }

        private void ListMusicFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = ListMusicFiles.SelectedItems;
            TxtFilename.IsReadOnly = false;
            if(selectedItems.Count == 0)
            {
                TxtFilename.Text = "";
                TxtTitle.Text = "";
                TxtArtist.Text = "";
                TxtAlbum.Text = "";
                TxtYear.Text = "";
                TxtTrack.Text = "";
                TxtAlbumArtist.Text = "";
                TxtGenre.Text = "";
                TxtComment.Text = "";
                TxtDuration.Text = "00:00:00";
                TxtFileSize.Text = "0 MB";
            }
            else if(selectedItems.Count == 1)
            {
                var musicFile = selectedItems[0] as MusicFile;
                TxtFilename.Text = musicFile.Filename;
                TxtTitle.Text = musicFile.Title;
                TxtArtist.Text = musicFile.Artist;
                TxtAlbum.Text = musicFile.Album;
                TxtYear.Text = musicFile.Year.ToString();
                TxtTrack.Text = musicFile.Track.ToString();
                TxtAlbumArtist.Text = musicFile.AlbumArtist;
                TxtGenre.Text = musicFile.Genre;
                TxtComment.Text = musicFile.Comment;
                TxtDuration.Text = musicFile.DurationAsString;
                TxtFileSize.Text = musicFile.FileSizeAsString;
            }
            else
            {
                var firstMusicFile = selectedItems[0] as MusicFile;
                var haveSameTitle = true;
                var haveSameArtist = true;
                var haveSameAlbum = true;
                var haveSameYear = true;
                var haveSameTrack = true;
                var haveSameAlbumArtist = true;
                var haveSameGenre = true;
                var haveSameComment = true;
                double totalDuration = 0;
                long totalFileSize = 0;
                foreach(var item in selectedItems)
                {
                    var musicFile = item as MusicFile;
                    if (firstMusicFile.Title != musicFile.Title)
                    {
                        haveSameTitle = false;
                    }
                    if (firstMusicFile.Artist != musicFile.Artist)
                    {
                        haveSameArtist = false;
                    }
                    if (firstMusicFile.Album != musicFile.Album)
                    {
                        haveSameAlbum = false;
                    }
                    if (firstMusicFile.Year != musicFile.Year)
                    {
                        haveSameYear = false;
                    }
                    if (firstMusicFile.Track != musicFile.Track)
                    {
                        haveSameTrack = false;
                    }
                    if (firstMusicFile.AlbumArtist != musicFile.AlbumArtist)
                    {
                        haveSameAlbumArtist = false;
                    }
                    if (firstMusicFile.Genre != musicFile.Genre)
                    {
                        haveSameGenre = false;
                    }
                    if (firstMusicFile.Comment != musicFile.Comment)
                    {
                        haveSameComment = false;
                    }
                    totalDuration += musicFile.Duration;
                    totalFileSize += musicFile.FileSize;
                }
                TxtFilename.IsReadOnly = true;
                TxtFilename.Text = "<keep>";
                TxtTitle.Text = haveSameTitle ? firstMusicFile.Title : "<keep>";
                TxtArtist.Text = haveSameArtist ? firstMusicFile.Artist : "<keep>";
                TxtAlbum.Text = haveSameAlbum ? firstMusicFile.Album : "<keep>";
                TxtYear.Text = haveSameYear ? firstMusicFile.Year.ToString() : "<keep>";
                TxtTrack.Text = haveSameTrack ? firstMusicFile.Track.ToString() : "<keep>";
                TxtAlbumArtist.Text = haveSameAlbumArtist ? firstMusicFile.AlbumArtist : "<keep>";
                TxtGenre.Text = haveSameGenre ? firstMusicFile.Genre : "<keep>";
                TxtComment.Text = haveSameComment ? firstMusicFile.Comment : "<keep>";
                TxtDuration.Text = totalDuration.DurationToString();
                TxtFileSize.Text = totalFileSize.FileSizeToString();
            }
        }
    }
}
