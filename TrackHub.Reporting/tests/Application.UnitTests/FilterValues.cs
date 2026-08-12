using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests;

// Test shorthand for FilterDto.Values (filter name → raw string value).
internal static class FilterValues
{
    public static Dictionary<string, string?> Of(params (string Name, string? Value)[] pairs)
    {
        var values = new Dictionary<string, string?>();
        foreach (var (name, value) in pairs)
        {
            values[name] = value;
        }
        return values;
    }
}
