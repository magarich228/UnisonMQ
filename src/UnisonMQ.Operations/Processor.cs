using System.Text;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class Processor : IOperationProcessor
{
    private readonly Operation[] _operations;
    private readonly List<byte> _buffer;
    private int _offset;

    internal Func<byte[], List<byte>, bool>? WaitAction { get; set; }
    internal Operation? Operation { get; set; }
    internal object? OperationContext { get; set; }
    
    public Processor(Operation[] operations)
    {
        _operations = operations;
        _buffer = new List<byte>(1024);
        WaitAction = DefaultWaitAction;
    }
    
    public void Execute(IUnisonMqSession session, byte[] data)
    {
        // TODO: Temp
        Console.WriteLine("data: " + Encoding.UTF8.GetString(data)); // TODO: на линухе pub приходит сразу вместе с телом сообщения
        
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
        Console.WriteLine(Encoding.UTF8.GetString(operationData)); //TODO: temp
        
        var result = Operation.ExecuteAsync(session, operationData, OperationContext);
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