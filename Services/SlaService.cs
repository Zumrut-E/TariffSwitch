public class SlaService
{
    private readonly TimeZoneInfo _viennaZone;

    public SlaService()
    {
        _viennaZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
    }

    public DateTimeOffset CalculateSla(DateTimeOffset requestedAt, string slaType, bool needsMeterUpgrade)
    {
        var localTime = TimeZoneInfo.ConvertTime(requestedAt, _viennaZone);

        int baseHours = slaType.Equals("Premium", StringComparison.OrdinalIgnoreCase) ? 24 : 48;

        if (needsMeterUpgrade)
        {
            baseHours += 12;
        }

        var deadlineLocal = localTime.DateTime.AddHours(baseHours);

        return new DateTimeOffset(deadlineLocal, _viennaZone.GetUtcOffset(deadlineLocal));
    }
}