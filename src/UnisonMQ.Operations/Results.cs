using System.Buffers;
using System.Text;

namespace UnisonMQ.Operations;

internal class Results
{
    public const string Ok = "+OK";
    public const string Error = "-ERR";
    public const string Message = "MSG";
}

internal static class ResultHelper
{
    public static string Ok() => $"{Results.Ok}\r\n";
    public static string Error(this string message) => $"{Results.Error} {message}\r\n";
    public static string Message(this string message, string subject, int sid, int messageLength) => 
        $"{Results.Message} {subject} {sid} {messageLength}\r\n{message}";

    public static byte[] MessageBytes(this byte[] message, string subject, int sid, int messageLength)
    {
        var messageSignature = Encoding.UTF8.GetBytes($"{Results.Message} {subject} {sid} {messageLength}\r\n");
        var result = ArrayPool<byte>.Shared.Rent(messageSignature.Length + messageLength);
        
        Array.Copy(result, messageSignature.Length, message, 0, messageLength);

        return result;
    }
}