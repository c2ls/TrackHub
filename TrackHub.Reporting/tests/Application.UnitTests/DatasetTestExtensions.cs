using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Application.UnitTests;

// Helpers to read a ReportDataset by column (property) name in unit tests.
internal static class DatasetTestExtensions
{
    public static int IndexOf(this ReportDataset dataset, string propertyName)
    {
        for (var i = 0; i < dataset.Columns.Count; i++)
        {
            if (dataset.Columns[i].PropertyName == propertyName)
            {
                return i;
            }
        }
        throw new ArgumentException($"Column '{propertyName}' not found.", nameof(propertyName));
    }

    public static object? Cell(this ReportDataset dataset, int row, string propertyName)
        => dataset.Rows[row][dataset.IndexOf(propertyName)];

    // Index of the first row whose <paramref name="propertyName"/> cell equals <paramref name="value"/>.
    // Lets aggregate reports be asserted by group key instead of by a position the grouping decides.
    public static int IndexOfRow(this ReportDataset dataset, string propertyName, object value)
    {
        var column = dataset.IndexOf(propertyName);
        for (var i = 0; i < dataset.Rows.Count; i++)
        {
            if (Equals(dataset.Rows[i][column], value))
            {
                return i;
            }
        }
        throw new ArgumentException($"No row with '{propertyName}' = '{value}'.", nameof(value));
    }

    public static IEnumerable<object?> Column(this ReportDataset dataset, string propertyName)
    {
        var index = dataset.IndexOf(propertyName);
        return dataset.Rows.Select(r => r[index]);
    }
}
