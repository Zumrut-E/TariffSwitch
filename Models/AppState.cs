namespace TariffSwitch.Processor.Models;

public class AppState
{
    public List<string> ProcessedRequestIds { get; set; } = new();
    public List<FollowUpAction> FollowUps { get; set; } = new();
}