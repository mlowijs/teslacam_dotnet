using CommandLine;

namespace TeslaCam.Options
{
    public class CommandLineOptions
    {
        [Option('q')]
        public bool Quiet { get; set; }
        
        [Option('c', "config")]
        public string? ConfigurationFilePath { get; set; }
        
        [Option('n', "notify")]
        public bool SendTestNotification { get; set; }
    }
}