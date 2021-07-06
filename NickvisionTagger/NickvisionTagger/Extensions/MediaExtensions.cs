using System;

namespace NickvisionTagger.Extensions
{
    public static class MediaExtensions
    {
        public static string DurationToString(this double totalSeconds)
        {
            var minutes = (int)totalSeconds / 60;
            var hours = minutes / 60;
            return $"{hours:D2}:{minutes % 60:D2}:{(int)totalSeconds % 60:D2}";
        }

        public static string FileSizeToString(this long fileSize)
        {
            string[] fileSizes = { "B", "KB", "MB", "GB", "TB" };
            double length = fileSize;
            var index = 0;
            while(length >= 1024 && index < 4)
            {
                index++;
                length /= 1024;
            }
            return $"{Math.Round(length, 2)} {fileSizes[index]}";
        }
    }
}
