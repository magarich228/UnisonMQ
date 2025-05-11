using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using UnisonMQ.Abstractions;
using UnisonMQ.Metrics;

namespace UnisonMQ.Operations;

internal abstract class Operation
{
    private byte[]? _keywordBytes;
    private byte[]? _lowerKeywordBytes;
    
    public abstract string Keyword { get; }

    public bool MatchKeyword(IEnumerable<byte> buffer)
    {
        _keywordBytes ??= Encoding.UTF8.GetBytes(Keyword.ToUpper());
        _lowerKeywordBytes ??= Encoding.UTF8.GetBytes(Keyword.ToLower());

        var immutableBuffer = buffer.ToImmutableArray();
        var bufferKeyword = immutableBuffer.Take(_keywordBytes.Length)
            .ToImmutableArray();

        var next = immutableBuffer.ElementAtOrDefault(_keywordBytes.Length);
        
        return (bufferKeyword.SequenceEqual(_keywordBytes) || 
                bufferKeyword.SequenceEqual(_lowerKeywordBytes)) &&
                (next == 32 || next == 13);
    }
    
    public abstract OperationResult ExecuteAsync(IUnisonMqSession session, byte[] data, object? context = null);
}

internal static class OperationExtensions
{
    internal static OperationResult TrackedExecuteAsync(
        this Operation operation, 
        IUnisonMqSession session, 
        byte[] data,
        object? context)
    {
        var sw = new Stopwatch();
        
        try
        {
            sw.Start();
            return operation.ExecuteAsync(session, data, context);
        }
        finally
        {
            sw.Stop();
            UnisonMetrics.TrackOperationDuration(operation.Keyword, sw.Elapsed);
        }
    }
}