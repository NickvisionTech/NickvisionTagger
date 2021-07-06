using NickvisionTagger.Extensions;
using System;

namespace NickvisionTagger.Models
{
    public class MusicFile : IComparable
    {
        private TagLib.File _file;

        public string Path { get; private set; }

        public MusicFile(string path)
        {
            Path = path;
            _file = TagLib.File.Create(Path);
        }

        public string Filename
        {
            get => System.IO.Path.GetFileName(Path);

            set
            {
                var newPath = Path.Remove(Path.IndexOf(Filename)) + value;
                if (!newPath.Contains(DotExtension))
                {
                    newPath += DotExtension;
                }
                _file.Dispose();
                _file = null;
                System.IO.File.Move(Path, newPath);
                Path = newPath;
                _file = TagLib.File.Create(Path);

            }
        }

        public string DotExtension => System.IO.Path.GetExtension(Path).ToLower();

        public string Title
        {
            get => _file.Tag.Title;

            set => _file.Tag.Title = value;
        }

        public string Artist
        {
            get => _file.Tag.FirstPerformer;

            set => _file.Tag.Performers = new string[] { value };
        }

        public string Album
        {
            get => _file.Tag.Album;

            set => _file.Tag.Album = value;
        }

        public uint Year
        {
            get => _file.Tag.Year;

            set => _file.Tag.Year = value;
        }

        public uint Track
        {
            get => _file.Tag.Track;

            set => _file.Tag.Track = value;
        }

        public string AlbumArtist
        {
            get => _file.Tag.FirstAlbumArtist;

            set => _file.Tag.AlbumArtists = new string[] { value };
        }

        public string Genre
        {
            get => _file.Tag.FirstGenre;

            set => _file.Tag.Genres = new string[] { value };
        }

        public string Comment
        {
            get => _file.Tag.Comment;

            set => _file.Tag.Comment = value;
        }

        public double Duration => _file.Properties.Duration.TotalSeconds;

        public string DurationAsString => Duration.DurationToString();

        public long FileSize => new System.IO.FileInfo(Path).Length;

        public string FileSizeAsString => FileSize.FileSizeToString();

        public void Save() => _file.Save();

        public void RemoveTag()
        {
            Title = "";
            Artist = "";
            Album = "";
            Year = 0;
            Track = 0;
            AlbumArtist = "";
            Genre = "";
            Comment = "";
            _file.Save();
        }

        public void FilenameToTag(string formatString)
        {
            var dashIndex = Filename.IndexOf('-');
            if (dashIndex == -1 && formatString != "%title%" && formatString != "%track% %title%")
            {
                throw new ArgumentException("Filename does not follow the format string");
            }
            var extensionIndex = Filename.ToLower().IndexOf(DotExtension);
            if (formatString == "%artist%- %title%")
            {
                Artist = Filename.Substring(0, dashIndex);
                Title = Filename.Substring(dashIndex + 2, extensionIndex - (Artist.Length + 2));
            }
            else if (formatString == "%title%- %artist%")
            {
                Title = Filename.Substring(0, dashIndex);
                Artist = Filename.Substring(dashIndex + 2, extensionIndex - (Title.Length + 2));
            }
            else if (formatString == "%track% %title%")
            {
                var spaceIndex = Filename.IndexOf(' ');
                if (spaceIndex == -1)
                {
                    throw new ArgumentException("Filename does not follow the format string");
                }
                Track = uint.Parse(Filename.Substring(0, spaceIndex));
                Title = Filename.Substring(spaceIndex + 1, extensionIndex - (Track.ToString().Length + 1));
            }
            else if (formatString == "%title%")
            {
                Title = Filename.Substring(0, extensionIndex);
            }
            _file.Save();
        }

        public void TagToFilename(string formatString)
        {
            if (formatString == "%artist%- %title%")
            {
                if (string.IsNullOrEmpty(Artist) || string.IsNullOrEmpty(Title))
                {
                    throw new ArgumentException("Artist and/or title fields are empty");
                }
                Filename = $"{Artist}- {Title}{DotExtension}";
            }
            else if (formatString == "%title%- %artist%")
            {
                if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Artist))
                {
                    throw new ArgumentException("Title and/or artist fields are empty");
                }
                Filename = $"{Title}- {Artist}{DotExtension}"; ;
            }
            else if (formatString == "%track% %title%")
            {
                if (Track == 0 || string.IsNullOrEmpty(Title))
                {
                    throw new ArgumentException("Track and/or title fields are empty");
                }
                Filename = $"{Track} {Title}{DotExtension}";
            }
            else if (formatString == "%title%")
            {
                if (string.IsNullOrEmpty(Title))
                {
                    throw new ArgumentException("Title field is empty");
                }
                Filename = $"{Title}{DotExtension}";
            }
        }

        public int CompareTo(object obj)
        {
            var toCompare = obj as MusicFile;
            if(toCompare == null)
            {
                throw new ArgumentException("Invalid MusicFile object to compare");
            }
            return string.Compare(Path, toCompare.Path);
        }
    }
}
