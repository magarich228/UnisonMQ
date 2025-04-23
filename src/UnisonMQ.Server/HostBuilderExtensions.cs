using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UnisonMQ.Server;

public static class HostBuilderExtensions
{
    public static IHostBuilder AddUnisonMq(this IHostBuilder hostBuilder)
    {
        hostBuilder
            .ConfigureServices(AddUnisonMq);
        
        return hostBuilder;
    }

    public static void AddUnisonMq(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        var configuration = new TcpServerConfiguration();
        hostBuilderContext.Configuration.Bind(configuration);
        
        services.AddSingleton(configuration);
        services.AddHostedService<TcpServerService>();
    }
}