using System.Globalization;
using TrackHub.Reporting.Domain.Helpers;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Application.UnitTests;

[TestFixture]
public class PdfReportBuilderTests
{
    // 1x1 transparent PNG.
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAC0lEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private static ReportDataset Dataset(int rows, byte[]? logo = null, params ReportColumn[] columns)
    {
        var cols = columns.Length > 0
            ? columns
            : [new ReportColumn("Status", typeof(string)), new ReportColumn("Speed", typeof(double)), new ReportColumn("Captured", typeof(DateTimeOffset))];

        object?[] BuildRow(int i)
        {
            var values = new object?[cols.Length];
            for (var c = 0; c < cols.Length; c++)
            {
                values[c] = cols[c].PropertyType == typeof(byte[])
                    ? Png
                    : cols[c].PropertyType == typeof(double)
                        ? i + 0.25
                        : cols[c].PropertyType == typeof(DateTimeOffset)
                            ? DateTimeOffset.UnixEpoch
                            : $"row{i}";
            }
            return values;
        }

        return new ReportDataset
        {
            Title = "PDF Report",
            FromDate = DateTimeOffset.UnixEpoch,
            ToDate = DateTimeOffset.UnixEpoch.AddDays(1),
            GeneratedAt = DateTimeOffset.UnixEpoch,
            Columns = cols,
            Rows = [.. Enumerable.Range(0, rows).Select(BuildRow)],
            AppliedFilters = [new KeyValuePair<string, string>("FilterFrom", "2026-01-01 00:00")],
            AccountName = logo is null ? null : "Acme Corp",
            LogoImage = logo
        };
    }

    [Test]
    public void Build_ProducesNonEmptyPdf_StartingWithSignature()
    {
        var bytes = new PdfReportBuilder().Build(Dataset(5), CultureInfo.GetCultureInfo("en"));

        Assert.That(bytes, Is.Not.Empty);
        Assert.That(System.Text.Encoding.ASCII.GetString(bytes, 0, 5), Is.EqualTo("%PDF-"));
    }

    [Test]
    public void Build_RendersSpanishHeaders_WithoutThrowing()
    {
        var bytes = new PdfReportBuilder().Build(Dataset(3), CultureInfo.GetCultureInfo("es"));
        Assert.That(bytes, Is.Not.Empty);
    }

    [Test]
    public void Build_WithImageCell_DoesNotThrow()
    {
        var columns = new[]
        {
            new ReportColumn("Status", typeof(string)),
            new ReportColumn("Photo", typeof(byte[]))
        };
        var bytes = new PdfReportBuilder().Build(Dataset(2, columns: columns), CultureInfo.InvariantCulture);
        Assert.That(bytes, Is.Not.Empty);
        Assert.That(System.Text.Encoding.ASCII.GetString(bytes, 0, 5), Is.EqualTo("%PDF-"));
    }

    [Test]
    public void Build_WithBrandingLogo_DoesNotThrow()
    {
        var bytes = new PdfReportBuilder().Build(Dataset(2, logo: Png), CultureInfo.InvariantCulture);
        Assert.That(bytes, Is.Not.Empty);
    }

    [Test]
    public void Build_EmptyDataset_DoesNotThrow()
    {
        var bytes = new PdfReportBuilder().Build(Dataset(0), CultureInfo.InvariantCulture);
        Assert.That(System.Text.Encoding.ASCII.GetString(bytes, 0, 5), Is.EqualTo("%PDF-"));
    }
}
