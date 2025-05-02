namespace UnisonMQ.Client;

internal struct Response
{
    public required string Keyword { get; set; }
    
    public static bool operator ==(Response left, Response right) => left.Keyword == right.Keyword;
    public static bool operator !=(Response left, Response right) => left.Keyword != right.Keyword;
    
    public static Response Ok { get; } = new() { Keyword = "+OK" };
    public static Response Error { get; } = new() { Keyword = "-ERR" };
    public static Response Pong { get; } = new() { Keyword = "PONG" };
    
    public bool Equals(Response other)
    {
        return Keyword == other.Keyword;
    }

    public override bool Equals(object? obj)
    {
        return obj is Response other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Keyword.GetHashCode();
    }
}