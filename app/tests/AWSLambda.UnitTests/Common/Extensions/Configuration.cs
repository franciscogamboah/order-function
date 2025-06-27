using Microsoft.Extensions.Configuration;
using Moq;

namespace AWSLambda.Tests.Common.Extensions;
public static class Configuration
{
    public static Mock<IConfigurationSection> SetConfiguration(this Mock<IConfigurationSection> configuration, string key, string value) 
    {
        configuration.Setup(s => s.Key).Returns(key);
        configuration.Setup(s => s.Value).Returns(value);
        return configuration;
    }
}
