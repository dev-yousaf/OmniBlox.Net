namespace OmniBlox.Shared.Extensions;

public static class DateTimeExtensions
{
    public static DateTime AsUtc(this DateTime date) => DateTime.SpecifyKind(date, DateTimeKind.Utc);

    public static DateTime? AsUtcOrNull(this DateTime? date) =>
        date.HasValue ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc) : null;
}
