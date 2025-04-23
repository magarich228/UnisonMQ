using Microsoft.Extensions.Hosting;
using UnisonMQ.Server;

var host = Host.CreateDefaultBuilder(args)
    .AddUnisonMq()
    .Build();

await host.RunAsync();