using System.Diagnostics.Metrics;

namespace UnisonMQ.Metrics;

public static class UnisonMetrics
{
    private static readonly Counter<long> MessagesReceived = 
        new Meter("UnisonMQ.Metrics")
            .CreateCounter<long>("messages.received");
    
    private static readonly ObservableGauge<int> ActiveConnections = 
        new Meter("UnisonMQ.Metrics")
            .CreateObservableGauge<int>(
                "active.connections", 
                () => Interlocked.CompareExchange(ref _connections, 0, 0));
    
    private static readonly Counter<long> BytesReceivedInternal = 
        new Meter("UnisonMQ.Metrics").CreateCounter<long>("network.bytes.received");
    
    

    
    private static int _connections;
    
    public static void MessageReceived() => MessagesReceived.Add(1);
    public static void ConnectionOpened() => Interlocked.Increment(ref _connections);
    public static void ConnectionClosed() => Interlocked.Decrement(ref _connections);
    public static void BytesReceived(int bytes) => BytesReceivedInternal.Add(bytes);
}
