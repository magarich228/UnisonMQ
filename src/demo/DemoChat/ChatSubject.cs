using System;
using DemoChat.Models;

namespace DemoChat;

public class ChatSubject<T>(string subject)
    where T : ChatEventBase
{
    public string Subject { get; } = subject;
    public Type MessageType => typeof(T);
}