namespace TariffSwitch.Processor.Models;

public class FollowUpAction
{
    public string RequestId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;
    // e.g. "Schedule meter upgrade"

    public DateTimeOffset DueAt { get; set; }
}


