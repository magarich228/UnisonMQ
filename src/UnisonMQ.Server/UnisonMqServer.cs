﻿using Microsoft.Extensions.Logging;
using NetCoreServer;
using UnisonMQ.Abstractions;
using UnisonMQ.Metrics;

namespace UnisonMQ.Server;

internal class UnisonMqServer : TcpServer
{
    private readonly ILogger<UnisonMqServer> _logger;
    private readonly ILogger<UnisonMqSession> _sessionLogger;

    private readonly ISessionManager _sessionManager;
    private readonly IQueueService _queueService;
    private readonly IOperationProcessorFactory _operationProcessorFactory;
    
    private readonly MetricsConfiguration _metricsConfiguration;

    public UnisonMqServer(
        TcpServerConfiguration configuration,
        MetricsConfiguration metricsConfiguration,
        ISessionManager sessionManager,
        IQueueService queueService,
        IOperationProcessorFactory operationProcessorFactory,
        ILoggerFactory loggerFactory) : base(configuration.Ip, configuration.Port)
    {
        _logger = loggerFactory.CreateLogger<UnisonMqServer>();
        _sessionLogger = loggerFactory.CreateLogger<UnisonMqSession>();
        
        _sessionManager = sessionManager;
        _queueService = queueService;
        _operationProcessorFactory = operationProcessorFactory;

        _metricsConfiguration = metricsConfiguration;
    }

    public UnisonMqServer(
        string address,
        int port,
        MetricsConfiguration metricsConfiguration,
        ISessionManager sessionManager,
        IQueueService queueService,
        IOperationProcessorFactory operationProcessorFactory,
        ILoggerFactory loggerFactory) 
        : this(
            new TcpServerConfiguration()
            {
                Ip = address,
                Port = port
            },
            metricsConfiguration,
            sessionManager,
            queueService,
            operationProcessorFactory,
            loggerFactory) { }

    public override bool Start()
    {
        _logger.LogInformation("Starting UnisonMQ server on {Address}:{Port}", Address, Port);
        _logger.LogInformation("Metrics on {MetricsIp}:{MetricsPort}", 
            _metricsConfiguration.MetricsIp, 
            _metricsConfiguration.MetricsPort);
        
        return base.Start();
    }

    public override bool Stop()
    {
        _logger.LogInformation("Stopping UnisonMQ server on {Address}:{Port}", Address, Port);
        
        return base.Stop();
    }

    protected override void OnConnected(TcpSession session)
    {
        _sessionManager.Add(session.Id, (IUnisonMqSession)session);
        
        base.OnConnected(session);
    }

    protected override void OnDisconnected(TcpSession session)
    {
        _sessionManager.Remove(session.Id);
        _queueService.Unsubscribe(session.Id, null);
        
        base.OnDisconnected(session);
    }

    protected override TcpSession CreateSession() => new UnisonMqSession(this, _operationProcessorFactory, _sessionLogger);
}