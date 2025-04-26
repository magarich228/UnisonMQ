namespace UnisonMQ.Operations;

internal class Results
{
    public const string Ok = "+OK";
    public const string Error = "-ERR";
}

internal static class ResultHelper
{
    public static string Ok() => $"{Results.Ok}\r\n";
    public static string Error(this string message) => $"{Results.Error} {message}\r\n";
}