using System;
using System.Globalization;
using Avalonia.Controls;

namespace DemoChat.Models;

public class ChatEventBase
{
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public virtual UserControl ToControl()
    {
        UserControl control = new UserControl();
        
        var timeStampBlock = new TextBlock
        {
            Text = Timestamp.ToString(CultureInfo.CurrentCulture)
        };

        control.Content = timeStampBlock;
        
        return control;
    }
}