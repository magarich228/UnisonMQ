using Avalonia.Controls;
using Avalonia.Interactivity;
using DemoChat.ViewModels;

namespace DemoChat.Views;

public partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;
    
    public MainWindow()
    {
        DataContext = new MainWindowViewModel();
        
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var chatWindow = new ChatWindow();
        
        chatWindow.Width = this.Width;
        chatWindow.Height = this.Height;
        chatWindow.Position = this.Position;
        
        chatWindow.ViewModel.Name = ViewModel.Name;

        chatWindow.Show();
        this.Close();
    }
}