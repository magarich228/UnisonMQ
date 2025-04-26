using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class Processor : IOperationProcessor
{
    private readonly Operation[] _operations;
    
    public Processor(Operation[] operations)
    {
        _operations = operations;
    }
    
    public void Execute(IUnisonMqSession session, string message)
    {
        var operation = _operations
            .FirstOrDefault(o => 
                message.StartsWith(o.Keyword, StringComparison.InvariantCultureIgnoreCase));

        if (operation == null)
        {
            session.SendAsync("Unknown Protocol operation.\r\n");
            session.Disconnect();
            
            return;
        }
        
        operation.ExecuteAsync(session, message);
    }
}