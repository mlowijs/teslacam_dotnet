using TeslaCam.Model;

namespace TeslaCam.Options
{
    public class TeslaCamOptions
    {
        public string RootDirectory { get; set; } = "/mnt/usbfs";
        public bool RootRequiresMounting { get; set; } = true;
        public int UploadInterval { get; set; } = 30;
        public ClipType[] ProcessClipTypes { get; set; } = new ClipType[0];
    }
}