﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using UnisonMQ.Abstractions;
using UnisonMQ.Metrics;
using UnisonMQ.Operations;
using UnisonMQ.Queues;

namespace UnisonMQ.Server;

public static class HostBuilderExtensions
{
    public static IHostBuilder AddUnisonMq(this IHostBuilder hostBuilder)
    {
        hostBuilder
            .ConfigureLogging(ConfigureLogging)
            .ConfigureServices(AddUnisonMq);
        
        return hostBuilder;
    }

    private static void ConfigureLogging(HostBuilderContext hostBuilderContext, ILoggingBuilder builder)
    {
        var configuration = hostBuilderContext.Configuration;

        builder.AddConfiguration(configuration);
    }
    
    private static void AddUnisonMq(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        var configuration = new TcpServerConfiguration();
        var metricsConfiguration = new MetricsConfiguration();
        
        hostBuilderContext.Configuration.Bind(configuration);
        hostBuilderContext.Configuration.Bind(metricsConfiguration);
        
        services.AddSingleton(configuration);
        services.AddSingleton(metricsConfiguration);
        services.AddSingleton(CreateServer);

        services.AddQueues();
        services.AddSingleton<ISessionManager, SessionManager>();
        services.AddOperations();
        
        services.AddHostedService<UnisonMqHostedService>();

        services.AddOpenTelemetry()
            .ConfigureResource(res => res.AddService("UnisonMQ"))
            .WithMetrics(m =>
            {
                m
                    .AddMeter(UnisonMetrics.MeterName)
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddPrometheusHttpListener(opt =>
                    {
                        opt.UriPrefixes = new[] { $"http://{metricsConfiguration.MetricsIp}:{metricsConfiguration.MetricsPort}/" };
                    });
            });
    }

    private static UnisonMqServer CreateServer(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<TcpServerConfiguration>();
        var metricsConfiguration = serviceProvider.GetRequiredService<MetricsConfiguration>();
        var sessionManager = serviceProvider.GetRequiredService<ISessionManager>();
        var queueService = serviceProvider.GetRequiredService<IQueueService>();
        var operationProcessorFactory = serviceProvider.GetRequiredService<IOperationProcessorFactory>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        
        var server = new UnisonMqServer(
            configuration,
            metricsConfiguration,
            sessionManager,
            queueService,
            operationProcessorFactory, 
            loggerFactory);
        
        return server;
    }
}