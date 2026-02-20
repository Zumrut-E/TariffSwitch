using TariffSwitch.Processor.Services;
using Xunit;

public class CsvServiceTests
{
    private readonly CsvService _csv = new();

    private string WriteTempFile(string content)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);
        return path;
    }

    // ── Customers ──────────────────────────────────────────────────────────

    [Fact]
    public void LoadCustomers_parses_valid_rows()
    {
        var path = WriteTempFile(
            "CustomerId;Name;HasUnpaidInvoice;Sla;MeterType\n" +
            "C001;Alice;FALSE;Premium;Smart");

        var result = _csv.LoadCustomers(path);

        Assert.Single(result);
        Assert.Equal("C001", result[0].CustomerId);
        Assert.False(result[0].HasUnpaidInvoice);
        Assert.Equal("Premium", result[0].Sla);
    }

    [Fact]
    public void LoadCustomers_throws_on_empty_file()
    {
        var path = WriteTempFile("CustomerId;Name;HasUnpaidInvoice;Sla;MeterType");
        Assert.Throws<InvalidOperationException>(() => _csv.LoadCustomers(path));
    }

    [Fact]
    public void LoadCustomers_skips_malformed_rows_silently()
    {
        var path = WriteTempFile(
            "CustomerId;Name;HasUnpaidInvoice;Sla;MeterType\n" +
            "C001;Alice;FALSE;Premium;Smart\n" +
            "BADROW");

        var result = _csv.LoadCustomers(path);
        Assert.Single(result); // bad row skipped, good row kept
    }

    // ── Tariffs ────────────────────────────────────────────────────────────

    [Fact]
    public void LoadTariffs_parses_valid_rows()
    {
        var path = WriteTempFile(
            "TariffId;Name;RequiresSmartMeter;BaseMonthlyGross\n" +
            "T-ECO;Eco Tariff;FALSE;29.99");

        var result = _csv.LoadTariffs(path);

        Assert.Single(result);
        Assert.Equal("T-ECO", result[0].TariffId);
        Assert.Equal(29.99m, result[0].BaseMonthlyGross);
        Assert.False(result[0].RequiresSmartMeter);
    }

    [Fact]
    public void LoadTariffs_skips_row_with_invalid_price()
    {
        var path = WriteTempFile(
            "TariffId;Name;RequiresSmartMeter;BaseMonthlyGross\n" +
            "T-ECO;Eco;FALSE;NOT_A_NUMBER\n" +
            "T-PRO;Pro;FALSE;49.99");

        var result = _csv.LoadTariffs(path);
        Assert.Single(result);
        Assert.Equal("T-PRO", result[0].TariffId);
    }

    // ── Requests ───────────────────────────────────────────────────────────

    [Fact]
    public void LoadRequests_parses_valid_rows()
    {
        var path = WriteTempFile(
            "RequestId;CustomerId;TargetTariffId;RequestedAtISO8601\n" +
            "R1001;C001;T-ECO;2025-03-30T01:15:00+01:00");

        var result = _csv.LoadRequests(path);

        Assert.Single(result);
        Assert.Equal("R1001", result[0].RequestId);
        Assert.Equal(TimeSpan.FromHours(1), result[0].RequestedAt.Offset);
    }

    [Fact]
    public void LoadRequests_bad_timestamp_sets_default_for_scenario7()
    {
        // Bad timestamp should not throw — validation service handles it as scenario 7
        var path = WriteTempFile(
            "RequestId;CustomerId;TargetTariffId;RequestedAtISO8601\n" +
            "R999;C001;T-ECO;NOT-A-DATE");

        var result = _csv.LoadRequests(path);

        Assert.Single(result);
        Assert.Equal(default, result[0].RequestedAt);
    }

    [Fact]
    public void LoadRequests_throws_on_missing_file()
    {
        Assert.Throws<FileNotFoundException>(() => _csv.LoadRequests("nonexistent.csv"));
    }

    [Fact]
    public void LoadRequests_throws_on_empty_file()
    {
        var path = WriteTempFile("RequestId;CustomerId;TargetTariffId;RequestedAtISO8601");
        Assert.Throws<InvalidOperationException>(() => _csv.LoadRequests(path));
    }
}