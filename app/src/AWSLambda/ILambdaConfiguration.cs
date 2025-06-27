using Microsoft.Extensions.Configuration;

namespace AWSLambda;
public interface ILambdaConfiguration
{
    IConfiguration Configuration { get; }
}
