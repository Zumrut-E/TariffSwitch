namespace TariffSwitch.Processor.Models;

public class ProcessingResult
{
    public string RequestId { get; set; } = string.Empty;

    public int ScenarioNumber { get; set; }
    public string ScenarioDescription { get; set; } = string.Empty;

    public DateTimeOffset? SlaDueAt { get; set; }
    public string? FollowUpAction { get; set; }
}



