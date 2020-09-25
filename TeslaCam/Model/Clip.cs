using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace TeslaCam.Model
{
    public class Clip
    {
        private const string DateTimeFormat = "yyyy-MM-dd_HH-mm-ss";
        
        private static readonly Regex DateTimeCameraRegex = new Regex(@"^(\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2})-(\w+)\.mp4$", RegexOptions.Compiled);
        
        public FileInfo File { get; }
        public ClipType Type { get; }
        public DateTimeOffset? EventDate { get; }
        public Camera Camera { get; }
        public DateTimeOffset Date { get; }
        
        public bool IsValid => Date != DateTimeOffset.MinValue
                               && Camera != Camera.Unknown
                               && File.Length > Constants.MegabyteInBytes;

        public Clip(FileInfo fileInfo, ClipType type, string? eventDirectoryName = null)
        {
            File = fileInfo;
            Type = type;

            if (eventDirectoryName != null)
                EventDate = DateTimeOffset.ParseExact(eventDirectoryName, DateTimeFormat, null, DateTimeStyles.AssumeUniversal);

            var regexMatch = DateTimeCameraRegex.Match(fileInfo.Name);

            if (!regexMatch.Success)
                return;
            
            Date = DateTimeOffset.ParseExact(regexMatch.Groups[1].Value, DateTimeFormat, null, DateTimeStyles.AssumeUniversal);

            Camera = regexMatch.Groups[2].Value switch
            {
                "front" => Camera.Front,
                "left_repeater" => Camera.LeftRepeater,
                "right_repeater" => Camera.RightRepeater,
                "back" => Camera.Back,
                _ => Camera.Unknown
            };
        }
    }
}