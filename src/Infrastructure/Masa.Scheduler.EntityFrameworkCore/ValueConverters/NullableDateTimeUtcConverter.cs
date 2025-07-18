namespace Masa.Scheduler.EntityFrameworkCore.ValueConverters;

public class NullableDateTimeUtcConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableDateTimeUtcConverter() : base(
        v => v.HasValue ? v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc).ToUniversalTime() : v.Value.ToUniversalTime() : null,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null)
    {
    }
}
