using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Layout;

namespace DemoChat.Models;

public class ChatMessage : ChatEventBase
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string Message { get; set; } = null!;

    public override UserControl ToControl()
    {
        UserControl control = new UserControl();
        StackPanel stackPanel = new StackPanel();

        stackPanel.Orientation = Orientation.Horizontal;
        stackPanel.Spacing = 5;
        control.Content = stackPanel;

        stackPanel.Children.Add(new TextBlock
        {
            Text = Timestamp.ToString(CultureInfo.CurrentCulture)
        });

        var messageTextBlock = new TextBlock
        {
            Text = $"{UserName}: {Message}",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        stackPanel.Children.Add(messageTextBlock);
        
        return control;
    }
}