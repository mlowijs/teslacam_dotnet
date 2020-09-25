using System;
using TeslaCam.Model;

namespace TeslaCam.Options
{
    public class TeslaCamOptions
    {
        public string RootDirectory { get; set; } = "/mnt/teslacam";
        public bool RootRequiresMounting { get; set; } = true;
        public string ArchiveDirectory { get; set; } = "/var/lib/teslacam/archive";
        
        public int UploadInterval { get; set; } = 30;
        
        public ClipType[] ClipTypesToProcess { get; set; } = new ClipType[0];
        public Camera[] CamerasToProcess { get; set; } = {Camera.Back, Camera.Front, Camera.LeftRepeater, Camera.RightRepeater};
        public int KeepClipsPerEventAmount { get; set; } = 10;

        public TimeSpan CleanTime { get; set; } = TimeSpan.FromHours(2);
    }
}