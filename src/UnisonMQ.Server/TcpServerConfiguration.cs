namespace UnisonMQ.Server;

internal sealed class TcpServerConfiguration
{
    public string Ip { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 5888;
}