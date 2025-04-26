namespace UnisonMQ.Abstractions;

public interface ISessionManager
{
    void Add(Guid clientId, IUnisonMqSession session);
    void Remove(Guid clientId);
}