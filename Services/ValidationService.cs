using TariffSwitch.Processor.Models;

namespace TariffSwitch.Processor.Services;

public class ValidationService
{
    // Returns a ProcessingResult if the request must be rejected (scenarios 4–7).
    // Returns null if the request is valid and can proceed to approval logic.
    public ProcessingResult? Validate(Request request, Customer? customer, Tariff? tariff)
    {
        // Scenario 7: Invalid request data (bad timestamp or malformed fields)
        if (string.IsNullOrWhiteSpace(request.RequestId) ||
            string.IsNullOrWhiteSpace(request.CustomerId) ||
            string.IsNullOrWhiteSpace(request.TargetTariff) ||
            request.RequestedAt == default)
        {
            return new ProcessingResult
            {
                RequestId = request.RequestId,
                ScenarioNumber = 7,
                ScenarioDescription = "Reject: Invalid request data"
            };
        }

        // Scenario 5: Unknown customer
        if (customer == null)
        {
            return new ProcessingResult
            {
                RequestId = request.RequestId,
                ScenarioNumber = 5,
                ScenarioDescription = "Reject: Unknown customer"
            };
        }

        // Scenario 6: Unknown tariff
        if (tariff == null)
        {
            return new ProcessingResult
            {
                RequestId = request.RequestId,
                ScenarioNumber = 6,
                ScenarioDescription = "Reject: Unknown tariff"
            };
        }

        // Scenario 4: Unpaid invoice
        if (customer.HasUnpaidInvoice)
        {
            return new ProcessingResult
            {
                RequestId = request.RequestId,
                ScenarioNumber = 4,
                ScenarioDescription = "Reject: Unpaid invoice"
            };
        }

        // Valid
        return null;
    }
}
