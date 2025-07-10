using Amazon.DynamoDBv2;
using Application.Common.Helpers;
using Application.Common.Infrastructure;
using Application.Common.Interfaces;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);
        AddGenericServices(services);
        return services;
    }
    private static void AddGenericServices(this IServiceCollection services)
    {
        services.AddAWSService<IAmazonDynamoDB>();
        services.AddTransient<IHelpers, Helpers>();
        services.AddTransient<IDynamoDbService, DynamoDbService>();
        services.AddTransient<IValidateTokenRepository, ValidateTokenRepository>();
    }
}
