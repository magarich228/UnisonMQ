using System.Text;
using Microsoft.Extensions.Logging;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class Processor : IOperationProcessor
{
    private readonly ILogger<Processor> _logger;
    
    private readonly Operation[] _operations;
    private readonly List<byte> _buffer;
    private int _offset;

    internal Func<byte[], List<byte>, bool>? WaitAction { get; set; }
    internal Operation? Operation { get; set; }
    internal object? OperationContext { get; set; }
    
    public Processor(Operation[] operations, ILogger<Processor> logger)
    {
        _operations = operations;
        _logger = logger;
        _buffer = new List<byte>(1024);
        WaitAction = DefaultWaitAction;
    }
    
    public void Execute(IUnisonMqSession session, byte[] data)
    {
        _buffer.AddRange(data);
        _offset += data.Length;

        if (!TryWait(data, _buffer, session))
        {
            return;
        }
        
        Operation ??= _operations.FirstOrDefault(o =>
            o.MatchKeyword(_buffer));
        
        if (Operation == null)
        {
            session.SendAsync("Unknown Protocol operation.".Error());
            session.Disconnect();
            
            return;
        }
        
        var operationData = _buffer.Take(_offset).ToArray();
        
        // TODO: temp or only development environment
        _logger.LogDebug("operation data: {OperationData}", Encoding.UTF8.GetString(operationData));
        
        var result = Operation.TrackedExecuteAsync(session, operationData, OperationContext);
        result.Apply(this);
        
        var remaining = _buffer.Skip(_offset);
        
        _buffer.Clear();
        _buffer.AddRange(remaining);
        
        _offset = 0;
    }

    private bool TryWait(byte[] data, List<byte> buffer, IUnisonMqSession session)
    {
        WaitAction ??= DefaultWaitAction;

        try
        {
            return WaitAction(data, buffer);
        }
        catch (UnisonMqException exception)
        {
            session.SendAsync(exception.Message.Error());
            session.Disconnect();

            return false;
        }
    }
    
    private bool DefaultWaitAction(byte[] data, List<byte> buffer)
    {
        var rIndex = Array.IndexOf(data, (byte)13);
        var nIndex = Array.IndexOf(data, (byte)10);

        return rIndex >= 0 && nIndex > 0 && nIndex == rIndex + 1;
    }
}