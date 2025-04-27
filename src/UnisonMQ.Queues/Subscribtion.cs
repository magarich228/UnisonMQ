namespace UnisonMQ.Queues;

internal class Subscription
{
    private readonly string _subject;
    private readonly object _balanceLock = new();
    private int? _messageBalance;
    
    public Subscription(string subject, int? messageBalance = null)
    {
        _subject = subject;
        _messageBalance = messageBalance;
    }
    
    public string Subject => _subject;

    public int? MessageBalance
    {
        get
        {
            lock (_balanceLock)
            {
                return _messageBalance;
            }
        }
    }

    public void DecrementBalance()
    {
        lock (_balanceLock)
        {
            if (_messageBalance is > 0)
            {
                _messageBalance--;
            }
        }
    }
    
    public bool IsMatch(string queueName) => WildcardMatcher.IsMatch(queueName, _subject);
}