namespace DemoConsole.Contracts;

public class TimeEvent
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public DateTime TimeStamp { get; set; } = DateTime.Now;
}