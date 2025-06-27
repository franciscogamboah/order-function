using Microsoft.Extensions.Configuration;

namespace AWSLambda;
public class LambdaConfiguration : ILambdaConfiguration
{
    public static IConfiguration Configuration => new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
              .Build();

    IConfiguration ILambdaConfiguration.Configuration => Configuration;
}

