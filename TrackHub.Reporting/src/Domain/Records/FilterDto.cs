using System.Globalization;

namespace TrackHub.Reporting.Domain.Records;

/// <summary>
/// The named filter values of a report request. <c>Values</c> maps filter name → raw
/// string value; the governed catalog's filter definitions (Manager `reports.filters`)
/// declare which names a report exposes and their types, and <see cref="FilterNames"/>
/// is the shared vocabulary. Every filter is optional by contract: an absent, empty or
/// unparseable value reads as null — "no filter" ("All" in the portal's pickers).
/// Accessors parse with the invariant culture; the portal sends dates as ISO-8601 and
/// numbers with a dot decimal separator.
/// </summary>
public sealed record FilterDto
{
    private static readonly IReadOnlyDictionary<string, string?> EmptyValues =
        new Dictionary<string, string?>();

    public string Name { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string?> Values { get; init; } = EmptyValues;

    public string? GetText(string name)
        => Values.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : null;

    // Guid.Empty reads as null: an all-zeros id is a cleared picker, not a filter.
    public Guid? GetGuid(string name)
        => Guid.TryParse(GetText(name), out var id) && id != Guid.Empty ? id : null;

    public DateTimeOffset? GetDate(string name)
        => DateTimeOffset.TryParse(GetText(name), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;

    public double? GetNumber(string name)
        => double.TryParse(GetText(name), NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            ? number
            : null;
}

/// <summary>
/// Canonical filter names — the same strings the Manager catalog seeds into each
/// report's filter definitions. A factory reading a name not listed in its catalog row
/// gets a filter the portal never renders; keep the two in lockstep.
/// </summary>
public static class FilterNames
{
    public const string Transporter = "transporterId";
    public const string Operator = "operatorId";
    public const string Geofence = "geofenceId";
    public const string Device = "deviceId";
    public const string Status = "status";
    public const string From = "from";
    public const string To = "to";
    public const string MaxRows = "maxRows";
    public const string WithinDays = "withinDays";
    public const string LookbackHours = "lookbackHours";
}
