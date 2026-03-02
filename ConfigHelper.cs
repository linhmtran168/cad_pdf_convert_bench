using Microsoft.Extensions.Configuration;

namespace CadBenchmark;

public static class ConfigHelper
{
    public static IConfiguration LoadConfig() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
}
