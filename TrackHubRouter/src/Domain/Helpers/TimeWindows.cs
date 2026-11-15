namespace TrackHub.Router.Domain.Helpers;

public static class TimeWindows
{
    // Padding a request by the widest UTC offset covers a provider that reads an undated range in its own zone.
    public static readonly TimeSpan LargestUtcOffset = TimeSpan.FromHours(14);

    // Consecutive windows start one second apart; contiguous ones share their boundary instant.
    public static IEnumerable<(DateTimeOffset Start, DateTimeOffset End)> Split(
        DateTimeOffset from, DateTimeOffset to, TimeSpan window, bool contiguous = false)
    {
        var start = from;
        while (contiguous ? start < to : start <= to)
        {
            var end = start + window < to ? start + window : to;
            yield return (start, end);
            start = contiguous ? end : end.AddSeconds(1);
        }
    }
}
