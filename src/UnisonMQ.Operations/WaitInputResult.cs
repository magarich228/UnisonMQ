using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class WaitInputResult : OperationResult
{
    private readonly object _context;
    private readonly long _inputLength;

    public WaitInputResult(object context, long inputLength)
    {
        _context = context;
        _inputLength = inputLength;
    }

    public override void Apply(Processor processor)
    {
        processor.OperationContext = _context;
        processor.WaitAction = WaitAction;
    }

    private bool WaitAction(byte[] data, List<byte> buffer)
    {
        if (buffer.Count > _inputLength + 2)
        {
            throw new UnisonMqException(
                "Protocol message body format error.");
        }
        
        return buffer.Count == _inputLength + 2 &&
               buffer.ElementAtOrDefault((int)_inputLength) == 13 &&
               buffer.ElementAtOrDefault((int)_inputLength + 1) == 10;
    }
}