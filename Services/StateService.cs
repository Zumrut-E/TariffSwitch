using System.Text.Json;
using TariffSwitch.Processor.Models;

namespace TariffSwitch.Processor.Services;

public class StateService
{
    private readonly string _statePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public StateService(string statePath)
    {
        _statePath = statePath;
        var dir = Path.GetDirectoryName(_statePath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);
    }

    public AppState Load()
    {
        if (!File.Exists(_statePath))
            return new AppState();

        var json = File.ReadAllText(_statePath);
        if (string.IsNullOrWhiteSpace(json))
            return new AppState();

        return JsonSerializer.Deserialize<AppState>(json, JsonOptions) ?? new AppState();
    }

    public void Save(AppState state)
    {
        // keep IDs unique
        state.ProcessedRequestIds = state.ProcessedRequestIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var json = JsonSerializer.Serialize(state, JsonOptions);
        File.WriteAllText(_statePath, json);
    }
}
