using TariffSwitch.Processor.Models;
using TariffSwitch.Processor.Services;

// Create services
var csv = new CsvService();
var validator = new ValidationService();
var sla = new SlaService();
var processor = new ProcessingService(validator, sla);

var stateService = new StateService("State/state.json");
var state = stateService.Load();
var processed = new HashSet<string>(state.ProcessedRequestIds, StringComparer.OrdinalIgnoreCase);

// Load CSVs
var customers = csv.LoadCustomers("Data/Customers.csv");
var tariffs = csv.LoadTariffs("Data/Tariffs.csv");
var requests = csv.LoadRequests("Data/Requests.csv");

// For fast lookup
var customersById = customers.ToDictionary(c => c.CustomerId, StringComparer.OrdinalIgnoreCase);
var tariffsById = tariffs.ToDictionary(t => t.TariffId, StringComparer.OrdinalIgnoreCase);

var results = new List<ProcessingResult>();

foreach (var req in requests)
{
    // Scenario 8: Skip already processed
    if (processed.Contains(req.RequestId))
    {
        results.Add(new ProcessingResult
        {
            RequestId = req.RequestId,
            ScenarioNumber = 8,
            ScenarioDescription = "Skip: Already processed"
        });
        continue;
    }

    customersById.TryGetValue(req.CustomerId, out var customer);
    tariffsById.TryGetValue(req.TargetTariff, out var tariff);

    var result = processor.Process(req, customer, tariff);
    results.Add(result);

    // Mark request as processed across runs (approved OR rejected)
    processed.Add(req.RequestId);
    state.ProcessedRequestIds.Add(req.RequestId);

    // Persist follow-up actions for scenario 3
    if (result.ScenarioNumber == 3 && result.SlaDueAt.HasValue && !string.IsNullOrWhiteSpace(result.FollowUpAction))
    {
        state.FollowUps.Add(new FollowUpAction
        {
            RequestId = req.RequestId,
            CustomerId = req.CustomerId,
            Action = result.FollowUpAction!,
            DueAt = result.SlaDueAt.Value
        });
    }
}

// Save state.json
stateService.Save(state);

// Print (your preferred scenario format)
foreach (var r in results)
{
    var due = r.SlaDueAt.HasValue ? $" due={r.SlaDueAt.Value:O}" : "";
    var follow = !string.IsNullOrWhiteSpace(r.FollowUpAction) ? $" follow-up={r.FollowUpAction}" : "";
    Console.WriteLine($"Scenario {r.ScenarioNumber}: {r.ScenarioDescription} → {r.RequestId}{due}{follow}");
}

