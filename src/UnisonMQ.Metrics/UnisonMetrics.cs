using System.Diagnostics.Metrics;

namespace UnisonMQ.Metrics;

public static class UnisonMetrics
{
    public const string MeterName = "UnisonMQ.Metrics";
    private static readonly Meter Meter = new(MeterName);
    
    private static readonly Counter<long> MessagesReceived = 
        Meter.CreateCounter<long>("messages.received");
    
    private static readonly ObservableGauge<int> ActiveConnections = 
        Meter.CreateObservableGauge<int>(
                "active.connections", 
                () => Interlocked.CompareExchange(ref _connections, 0, 0));
    
    private static int _connections;
    
    private static readonly Counter<long> BytesReceivedInternal = 
        Meter.CreateCounter<long>("network.bytes.received");
    
    // ObservableGauge after implementing persistent storage
    private static readonly Counter<long> MessagesPublished = 
        Meter.CreateCounter<long>(
            name: "messages.published",
            unit: "messages",
            description: "Total messages published per subject");
    
    private static readonly Histogram<double> OperationDuration = Meter.CreateHistogram<double>(
        name: "operations.duration",
        unit: "ms",
        description: "Duration of UnisonMQ operations",
        advice: new InstrumentAdvice<double>()
        {
            HistogramBucketBoundaries = new[]
                { 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.75, 1, 2, 3, 5, 10, 20, 30, 50, 100, 200, 500, 1000, 10000 }
        });
    
    public static void MessageReceived() => MessagesReceived.Add(1);
    public static void ConnectionOpened() => Interlocked.Increment(ref _connections);
    public static void ConnectionClosed() => Interlocked.Decrement(ref _connections);
    public static void BytesReceived(int bytes) => BytesReceivedInternal.Add(bytes);
    public static void MessagePublished(string subject) =>
        MessagesPublished.Add(1, new KeyValuePair<string, object?>("subject", subject));
    public static void TrackOperationDuration(string operationName, TimeSpan duration) =>
        OperationDuration.Record(
            duration.TotalMilliseconds,
            tag: new("operation", operationName));
}
