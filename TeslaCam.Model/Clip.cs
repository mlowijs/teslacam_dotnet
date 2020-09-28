using System;
using System.IO;

namespace TeslaCam.Model
{
    public class Clip
    {
        public FileInfo File { get; set; }
        public ClipType Type { get; set; }
        public DateTimeOffset? EventDate { get; set; }
        public Camera Camera { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}