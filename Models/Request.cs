namespace TariffSwitch.Processor.Models;

public class Request
{
    // Scenario 8: This ID is used to check against processed_ids.txt
    public string RequestId { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public string TargetTariff { get; set; } = string.Empty;

    // Requirement: This must be in ISO-8601 format (e.g., 2025-03-30T01:15:00+01:00)
    public DateTimeOffset RequestedAt { get; set; }


}