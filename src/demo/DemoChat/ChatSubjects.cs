using DemoChat.Models;

namespace DemoChat;

public static class ChatSubjects
{
    public const string ChatGlobal = "chat.*";

    public static ChatSubject<NewUser> NewUserSubject = new ChatSubject<NewUser>("chat.newuser");
    public static ChatSubject<UserExit> UserExitSubject = new ChatSubject<UserExit>("chat.userexit");
    public static ChatSubject<ChatMessage> ChatMessageSubject = new ChatSubject<ChatMessage>("chat.messages");
}