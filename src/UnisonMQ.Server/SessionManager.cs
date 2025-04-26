using System.Collections.Concurrent;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Server;

public class SessionManager : ISessionManager
{
    private readonly ConcurrentDictionary<Guid, IUnisonMqSession> _sessions = new();
    
    public void Add(Guid clientId, IUnisonMqSession session)
    {
        if (!_sessions.TryAdd(clientId, session))
        {
            throw new UnisonMqException("Failed to add session.");
        }
    }

    public void Remove(Guid clientId)
    {
        if (!_sessions.TryRemove(clientId, out _))
        {
            throw new UnisonMqException("Failed to remove session.");
        }
    }
}