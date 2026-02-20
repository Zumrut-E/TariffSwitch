using TariffSwitch.Processor.Models;

namespace TariffSwitch.Processor.Services;

public class ProcessingService
{
    private readonly ValidationService _validator;
    private readonly SlaService _slaService;

    public ProcessingService(ValidationService validator, SlaService slaService)
    {
        _validator = validator;
        _slaService = slaService;
    }

    public ProcessingResult Process(Request request, Customer? customer, Tariff? tariff)
    {
        // First: rejection scenarios 4–7
        var rejection = _validator.Validate(request, customer, tariff);
        if (rejection != null)
            return rejection;

        // Now safe to use:
        var c = customer!;
        var t = tariff!;

        // Scenario 3 condition:
        // tariff requires smart meter AND customer does NOT have smart meter
        bool hasSmart = c.MeterType.Equals("Smart", StringComparison.OrdinalIgnoreCase);
        bool needsUpgrade = t.RequiresSmartMeter && !hasSmart;

        // SLA due time (Vienna + DST safe)
        var dueAt = _slaService.CalculateSla(request.RequestedAt, c.Sla, needsUpgrade);

        if (needsUpgrade)
        {
            return new ProcessingResult
            {
                RequestId = request.RequestId,
                ScenarioNumber = 3,
                ScenarioDescription = "Approve: Smart meter required; schedule meter upgrade",
                SlaDueAt = dueAt,
                FollowUpAction = "Schedule meter upgrade"
            };
        }

        // Scenario 2 vs 1 depends on SLA type
        bool isPremium = c.Sla.Equals("Premium", StringComparison.OrdinalIgnoreCase);

        if (isPremium)
        {
            return new ProcessingResult
            {
                RequestId = request.RequestId,
                ScenarioNumber = 2,
                ScenarioDescription = "Approve: Premium SLA; no follow-up actions",
                SlaDueAt = dueAt
            };
        }

        return new ProcessingResult
        {
            RequestId = request.RequestId,
            ScenarioNumber = 1,
            ScenarioDescription = "Approve: Standard SLA; no follow-up actions",
            SlaDueAt = dueAt
        };
    }
}
