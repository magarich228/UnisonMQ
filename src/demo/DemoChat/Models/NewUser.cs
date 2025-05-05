using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Layout;

namespace DemoChat.Models;

public class NewUser : ChatEventBase
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

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

        var newUserTextBlock = new TextBlock
        {
            Text = $"{Name} подключился к чату",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        stackPanel.Children.Add(newUserTextBlock);
        
        return control;
    }
}