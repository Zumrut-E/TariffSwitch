using System.Globalization;
using TariffSwitch.Processor.Models;

namespace TariffSwitch.Processor.Services;

public class CsvService
{
    public List<Customer> LoadCustomers(string path)
    {
        var lines = File.ReadAllLines(path);
        if (lines.Length < 2)
            throw new InvalidOperationException($"customers.csv is empty or missing data rows: {path}");

        var customers = new List<Customer>();

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(';');
            if (parts.Length < 5) continue;

            customers.Add(new Customer
            {
                CustomerId = parts[0].Trim(),
                Name = parts[1].Trim(),
                HasUnpaidInvoice = ParseBool(parts[2]),
                Sla = parts[3].Trim(),
                MeterType = parts[4].Trim()
            });
        }

        return customers;
    }

    public List<Tariff> LoadTariffs(string path)
    {
        var lines = File.ReadAllLines(path);
        if (lines.Length < 2)
            throw new InvalidOperationException($"tariffs.csv is empty or missing data rows: {path}");

        var tariffs = new List<Tariff>();

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(';');
            if (parts.Length < 4) continue;

            if (!decimal.TryParse(parts[3].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var monthlyGross))
                continue;

            tariffs.Add(new Tariff
            {
                TariffId = parts[0].Trim(),
                Name = parts[1].Trim(),
                RequiresSmartMeter = ParseBool(parts[2]),
                BaseMonthlyGross = monthlyGross
            });
        }

        return tariffs;
    }

    private static bool ParseBool(string raw)
    {
        var s = raw.Trim();
        if (s.Equals("TRUE", StringComparison.OrdinalIgnoreCase)) return true;
        if (s.Equals("FALSE", StringComparison.OrdinalIgnoreCase)) return false;

        if (bool.TryParse(s, out var b)) return b;
        if (s == "1") return true;
        if (s == "0") return false;

        throw new FormatException($"Invalid boolean value: '{raw}'");
    }
public List<Request> LoadRequests(string path)
{
    var lines = File.ReadAllLines(path);

    if (lines.Length < 2)
        throw new InvalidOperationException($"requests.csv is empty or missing data rows: {path}");

    var requests = new List<Request>();

    foreach (var line in lines.Skip(1))
    {
        if (string.IsNullOrWhiteSpace(line))
            continue;

        var parts = line.Split(';');
        if (parts.Length < 4)
            continue; // malformed row

        // parse timestamp safely
        if (!DateTimeOffset.TryParse(parts[3].Trim(), out var requestedAt))
        {
            // invalid timestamp → store default, validation will catch it (Scenario 7)
            requestedAt = default;
        }

        requests.Add(new Request
        {
            RequestId = parts[0].Trim(),
            CustomerId = parts[1].Trim(),
            TargetTariff = parts[2].Trim(),
            RequestedAt = requestedAt
        });
    }

    return requests;
}

}


