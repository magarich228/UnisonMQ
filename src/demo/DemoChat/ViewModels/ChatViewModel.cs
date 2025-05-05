using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using DemoChat.Models;
using UnisonMQ.Client;

namespace DemoChat.ViewModels;

public partial class ChatViewModel : ViewModelBase
{
    private readonly IUnisonMqClient _unisonMqClient = new UnisonMqClientService(new UnisonMqConfiguration());

    private string? _message;
    
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = $"Guest-{Guid.NewGuid()}";
    
    public string? Message
    {
        get => _message;
        set
        {
            if (_message == value) return;
            _message = value;
            OnPropertyChanged();
        }
    }
    
    public ObservableCollection<UserControl> Events { get; } = new();
    
    public bool ConnectToServer()
    {
        var connected = _unisonMqClient.ConnectAsync();

        if (connected)
        {
            InitializeChat();
        }

        return connected;
    }
    
    public bool CloseConnection()
    {
        _unisonMqClient.Publish(ChatSubjects.UserExitSubject.Subject, new UserExit()
        {
            Id = Id,
            Name = Name
        });
        return _unisonMqClient.CloseAsync();
    }

    public void SendMessage()
    {
        _unisonMqClient.Publish(ChatSubjects.ChatMessageSubject.Subject, new ChatMessage()
        {
            Message = Message ?? string.Empty,
            UserId = Id,
            UserName = Name
        });
    }

    private void OnMessageReceived(UnisonMessage<ChatEventBase> message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Events.Add(message.Data.ToControl());
        });
    }

    private void InitializeChat()
    {
        Task.Run(() =>
        {
            _unisonMqClient.Subscribe<ChatEventBase>(ChatSubjects.ChatGlobal, OnMessageReceived);
            _unisonMqClient.Publish(ChatSubjects.NewUserSubject.Subject, new NewUser()
            {
                Id = Id,
                Name = Name
            });
        });
    }
}