namespace UnisonMQ.Client;

internal abstract class UnisonMessageFactory
{
    public abstract void CreateAndInvoke(string subject, byte[] data);
}