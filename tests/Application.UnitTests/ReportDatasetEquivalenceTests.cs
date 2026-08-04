using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests;

// Dataset-equivalence tests: for a representative VM per report family, the dataset's
// columns must equal the VM's public property names in declaration order, and each row must carry the
// same values the pre-refactor ClosedXML pipeline exported (it reflected the same properties).
[TestFixture]
public class ReportDatasetEquivalenceTests
{
    private static FilterDto Filters() => new()
    {
        Name = "My Report",
        Language = "en",
        Values = new Dictionary<string, string?>
        {
            [FilterNames.From] = "2026-01-01T00:00:00Z",
            [FilterNames.To] = "2026-01-31T00:00:00Z",
            [FilterNames.Status] = "abc",
            [FilterNames.MaxRows] = "7",
            [FilterNames.Transporter] = null
        }
    };

    private static void AssertMatchesProperties<T>(IReadOnlyCollection<T> rows)
    {
        var properties = typeof(T).GetProperties();
        var dataset = ReportDataset.Create(Filters(), rows);

        // Columns == property names, in declaration order.
        Assert.That(dataset.Columns.Select(c => c.PropertyName), Is.EqualTo(properties.Select(p => p.Name)));
        Assert.That(dataset.Columns.Select(c => c.PropertyType), Is.EqualTo(properties.Select(p => p.PropertyType)));

        // Rows carry the same values the old pipeline projected.
        Assert.That(dataset.RowCount, Is.EqualTo(rows.Count));
        var rowList = rows.ToList();
        for (var r = 0; r < rowList.Count; r++)
        {
            for (var c = 0; c < properties.Length; c++)
            {
                Assert.That(dataset.Rows[r][c], Is.EqualTo(properties[c].GetValue(rowList[r])));
            }
        }

        // Title + date range come from the filter.
        Assert.That(dataset.Title, Is.EqualTo("My Report"));
        Assert.That(dataset.FromDate, Is.EqualTo(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        Assert.That(dataset.ToDate, Is.EqualTo(new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero)));
    }

    [Test]
    public void Legacy_PositionVm_ColumnsAndValuesMatch()
        => AssertMatchesProperties(new[]
        {
            new PositionVm(Guid.NewGuid(), "Dev", "Truck", 4.5, -74.1, 10.0,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 60, 90, 1, "addr", "city", "state", "country")
        });

    [Test]
    public void Gps_ProviderHealthRowVm_ColumnsAndValuesMatch()
        => AssertMatchesProperties(new[]
        {
            new GpsProviderHealthRowVm(Guid.NewGuid(), "Op1", "Healthy", 0.99, 25.0, 0, DateTimeOffset.UtcNow, null)
        });

    [Test]
    public void Admin_AccountByStatusRowVm_ColumnsAndValuesMatch()
        => AssertMatchesProperties(new[]
        {
            new AccountByStatusRowVm("Acme", "Active", 1, true, DateTimeOffset.UtcNow)
        });

    [Test]
    public void Document_ExpiringDocumentRowVm_ColumnsAndValuesMatch()
        => AssertMatchesProperties(new[]
        {
            new ExpiringDocumentRowVm("SOAT", "Transporter", "t1", "a.pdf", "Internal", "Active", DateTimeOffset.UtcNow)
        });

    [Test]
    public void AppliedFilters_EchoProvidedValuesUnderNamedKeys()
    {
        var dataset = ReportDataset.Create(Filters(), new[] { new AccountByStatusRowVm("A", "Active", 1, true, DateTimeOffset.UtcNow) });
        var applied = dataset.AppliedFilters.ToDictionary(f => f.Key, f => f.Value);

        // Named echo: "Filter" + PascalCase(name); dates normalized, others raw.
        Assert.That(applied["FilterFrom"], Is.EqualTo("2026-01-01 00:00"));
        Assert.That(applied["FilterTo"], Is.EqualTo("2026-01-31 00:00"));
        Assert.That(applied["FilterStatus"], Is.EqualTo("abc"));
        Assert.That(applied["FilterMaxRows"], Is.EqualTo("7"));
        // Null/empty values are not echoed.
        Assert.That(applied.ContainsKey("FilterTransporterId"), Is.False);
    }
}
