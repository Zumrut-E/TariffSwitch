using TariffSwitch.Processor.Services;
using TariffSwitch.Processor.Models;
using Xunit;

public class StateServiceTests
{
    private string TempStatePath() =>
        Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "state.json");

    [Fact]
    public void Load_returns_empty_state_when_file_does_not_exist()
    {
        var service = new StateService(TempStatePath());
        var state = service.Load();

        Assert.NotNull(state);
        Assert.Empty(state.ProcessedRequestIds);
        Assert.Empty(state.FollowUps);
    }

    [Fact]
    public void Save_then_Load_roundtrips_correctly()
    {
        var path = TempStatePath();
        var service = new StateService(path);

        var state = new AppState();
        state.ProcessedRequestIds.Add("R001");
        state.ProcessedRequestIds.Add("R002");
        service.Save(state);

        var loaded = service.Load();
        Assert.Contains("R001", loaded.ProcessedRequestIds);
        Assert.Contains("R002", loaded.ProcessedRequestIds);
    }

    [Fact]
    public void Save_deduplicates_ids_case_insensitively()
    {
        var path = TempStatePath();
        var service = new StateService(path);

        var state = new AppState();
        state.ProcessedRequestIds.AddRange(["R001", "r001", "R001"]);
        service.Save(state);

        var loaded = service.Load();
        Assert.Single(loaded.ProcessedRequestIds);
    }

    [Fact]
    public void Save_persists_followups_with_correct_deadline()
    {
        var path = TempStatePath();
        var service = new StateService(path);
        var due = new DateTimeOffset(2025, 6, 16, 11, 20, 0, TimeSpan.FromHours(2));

        var state = new AppState();
        state.FollowUps.Add(new FollowUpAction
        {
            RequestId = "R001",
            CustomerId = "C001",
            Action = "Schedule meter upgrade",
            DueAt = due
        });
        service.Save(state);

        var loaded = service.Load();
        Assert.Single(loaded.FollowUps);
        Assert.Equal(due, loaded.FollowUps[0].DueAt);
        Assert.Equal("R001", loaded.FollowUps[0].RequestId);
    }

    [Fact]
    public void Load_returns_empty_state_on_blank_file()
    {
        var path = TempStatePath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "   ");

        var service = new StateService(path);
        var state = service.Load();

        Assert.Empty(state.ProcessedRequestIds);
    }
}