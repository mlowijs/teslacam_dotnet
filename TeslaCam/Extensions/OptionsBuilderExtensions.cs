using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace TeslaCam.Extensions
{
    public static class OptionsBuilderExtensions
    {
        private const string Options = "Options";
        
        public static OptionsBuilder<TOptions> ConfigureSection<TOptions>(this OptionsBuilder<TOptions> builder)
            where TOptions: class
        {
            var section = typeof(TOptions).Name.Replace(Options, string.Empty);
            
            return builder.Configure<IConfiguration>((options, configuration) =>
                configuration.GetSection(section).Bind(options));
        }
    }
}