using System.Globalization;
using System.Reflection;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

public static class Processor
{
    private static readonly Operation[] Operations;
    
    static Processor()
    {
        Operations = CollectOperations();
    }
    
    public static void Execute(IUnisonMqSession session, string message)
    {
        var operation = Operations
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

    private static Operation[] CollectOperations()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var operationType = typeof(Operation);

        var operations = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(operationType))
            .Select(t => (Operation)Activator.CreateInstance(t)! ?? 
                         throw new UnisonMqException("Failed to create operation"));

        return operations.ToArray();
    }
}