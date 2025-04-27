namespace UnisonMQ.Abstractions;

public interface ISessionManager
{
    bool TryGet(Guid clientId, out IUnisonMqSession? session);
    void Add(Guid clientId, IUnisonMqSession session);
    void Remove(Guid clientId);
}