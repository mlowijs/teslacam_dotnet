using System;
using TeslaCam.Model;

namespace TeslaCam.Options
{
    public class TeslaCamOptions
    {
        public string MountPoint { get; set; } = "/mnt/teslacam";
        public bool MountingRequired { get; set; } = true;
        public string DataDirectory { get; set; } = "/var/lib/teslacam";
        
        public ClipType[] ClipTypesToProcess { get; set; } = new ClipType[0];
        public Camera[] CamerasToProcess { get; set; } = {Camera.Back, Camera.Front, Camera.LeftRepeater, Camera.RightRepeater};
        public int KeepClipsPerEventAmount { get; set; } = 10;

        public TimeSpan CleanTime { get; set; } = TimeSpan.FromHours(2);

        public bool ManageSentryMode { get; set; } = false;

        public string Uploader { get; set; } = "";
    }
}