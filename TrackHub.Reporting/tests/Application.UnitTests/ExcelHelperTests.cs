using System.Globalization;
using ClosedXML.Excel;
using TrackHub.Reporting.Domain.Exceptions;
using TrackHub.Reporting.Domain.Helpers;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Options;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests;

[TestFixture]
public class ExcelHelperTests
{
    // Property names Status/Speed exist in the resx; "Captured" does not (falls back to the key).
    private readonly record struct SampleRow(string Status, double Speed, DateTimeOffset Captured);

    private static ReportDataset Dataset(int rows)
        => ReportDataset.Create(
            new FilterDto { Name = "Sample Report", Language = "en" },
            [.. Enumerable.Range(0, rows).Select(i => new SampleRow($"s{i}", i + 0.5, DateTimeOffset.UnixEpoch))]);

    [Test]
    public void Export_ProducesReadableWorkbook_WithTitleHeadersAndRows()
    {
        var helper = new ExcelHelper(new ReportingLimitsOptions());

        var bytes = helper.Export(Dataset(3), CultureInfo.GetCultureInfo("en"));

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        Assert.That(worksheet.Name, Is.EqualTo("Report"));
        Assert.That(worksheet.Cell("A1").GetString(), Does.Contain("Sample Report"));
        // Header row (row 2) resolves through the resx (Status is mapped; Captured falls back to the key).
        Assert.That(worksheet.Cell(2, 1).GetString(), Is.EqualTo("Status"));
        Assert.That(worksheet.Cell(2, 2).GetString(), Is.Not.Empty);
        Assert.That(worksheet.Cell(2, 3).GetString(), Is.EqualTo("Captured"));
        // First data row (row 3).
        Assert.That(worksheet.Cell(3, 1).GetString(), Is.EqualTo("s0"));
    }

    [Test]
    public void Export_OverMaxExportRows_ThrowsLimitExceeded()
    {
        var helper = new ExcelHelper(new ReportingLimitsOptions { MaxExportRows = 2 });

        var ex = Assert.Throws<ReportLimitExceededException>(
            () => helper.Export(Dataset(3), CultureInfo.InvariantCulture));
        Assert.That(ex!.MaxRows, Is.EqualTo(2));
    }

    [Test]
    public void Export_EmptyDataset_StillProducesReadableWorkbook()
    {
        var helper = new ExcelHelper(new ReportingLimitsOptions());

        var bytes = helper.Export(Dataset(0), CultureInfo.InvariantCulture);

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        Assert.That(workbook.Worksheet(1).Cell(2, 1).GetString(), Is.EqualTo("Status"));
    }
}
