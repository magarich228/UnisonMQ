using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DemoChat.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace DemoChat.Views;

public partial class ChatWindow : Window
{
    public ChatViewModel ViewModel => (ChatViewModel)DataContext!;
    
    public ChatWindow()
    {
        DataContext = new ChatViewModel();
        
        InitializeComponent();
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        var connected = ViewModel.ConnectToServer();

        if (!connected)
        {
            var mainWindow = new MainWindow();
            ReturnToMain(mainWindow);
            
            MessageBoxManager.GetMessageBoxStandard(
                    "Подключение к чату", 
                    $"Подключение к серверу: {connected}", 
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowWindowDialogAsync(mainWindow);
        }
        
        base.OnLoaded(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        ViewModel.CloseConnection();
        
        base.OnClosed(e);
    }

    private void ReturnToMain(MainWindow? mainWindow = null)
    {
        mainWindow ??= new MainWindow();
        mainWindow.ViewModel.Name = ViewModel.Name;

        mainWindow.Width = this.Width;
        mainWindow.Height = this.Height;
        mainWindow.Position = this.Position;
        
        mainWindow.Show();
        this.Close();
    }

    private void Exit_OnLick(object? sender, RoutedEventArgs e)
    {
        ReturnToMain();
    }

    private void Send_OnClick(object sender, RoutedEventArgs e)
    {
        var message = ViewModel.Message;

        if (!string.IsNullOrWhiteSpace(message))
        {
            ViewModel.SendMessage();
            ViewModel.Message = string.Empty;
        }
    }
}