using Application;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AWSLambda;
public class Startup
{
    public IConfiguration _configuration { get; }

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static IServiceCollection Container => ConfigureServices(LambdaConfiguration.Configuration);

    public static IServiceCollection ConfigureServices(IConfiguration root)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure(root);

        services.AddLogging(logginBuilder =>
        {
            logginBuilder.ClearProviders();
        });

        return services;

    }
}