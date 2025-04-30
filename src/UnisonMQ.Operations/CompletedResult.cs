namespace UnisonMQ.Operations;

internal class CompletedResult : OperationResult
{
    private static readonly CompletedResult ResultInstance = new();
    public static CompletedResult Instance = ResultInstance;
    
    public override void Apply(Processor processor)
    {
        processor.Operation = null;
        processor.OperationContext = null;
        processor.WaitAction = null;
    }
}