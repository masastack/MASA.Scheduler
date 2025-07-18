namespace Masa.Scheduler.EntityFrameworkCore.ValueConverters;

public class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
{
    public DateTimeUtcConverter() : base(
        v => v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}