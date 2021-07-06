using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NickvisionTagger.Models
{
    public class MusicFolder
    {
        public string Path { get; set; }
        public bool IncludeSubfolders { get; set; }
        public List<MusicFile> Files { get; init; }
        
        public MusicFolder()
        {
            Path = "";
            IncludeSubfolders = true;
            Files = new List<MusicFile>();
        }

        public async Task RescanFilesAsync()
        {
            string[] extensions = { ".mp3", ".wav", ".wma", ".ogg", ".flac" };
            var searchOption = IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            Files.Clear();
            if(Directory.Exists(Path))
            {
                await Task.Run(() =>
                {
                    foreach (var file in Directory.EnumerateFiles(Path, "*.*", searchOption))
                    {
                        if (extensions.Contains(System.IO.Path.GetExtension(file).ToLower()))
                        {
                            Files.Add(new MusicFile(file));
                        }
                    }
                    Files.Sort();
                });
            }
        }
    }
}
