using Moq;
using TrackHub.Reporting.Application.Report.Factory.Document;
using TrackHub.Reporting.Application.UnitTests;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests.Documents;

[TestFixture]
public class DocumentReportsTests
{
    private Mock<IDocumentReportReader> _reader = null!;
    private static readonly Guid DocumentId = Guid.NewGuid();
    private Mock<IReportNameReader> _names = null!;
    private FilterDto _filters;

    [SetUp]
    public void SetUp()
    {
        _names = new Mock<IReportNameReader>();
        _names.Setup(n => n.GetTransporterNamesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string>());
        _names.Setup(n => n.GetOperatorNamesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string>());
        _names.Setup(n => n.GetDeviceSerialsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string>());
        _names.Setup(n => n.GetStaffNamesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string>());
        _names.Setup(n => n.GetDocumentNamesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string>());
        _reader = new Mock<IDocumentReportReader>();
        _reader.Setup(r => r.EnsureDocumentsFeatureAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _filters = new FilterDto { Language = "en", Name = "Test" };
    }

    private static IReadOnlyCollection<T> List<T>(params T[] items) => items;

    [Test]
    public void Codes_MatchSpecConstants()
    {
        Assert.That(new ExpiringDocumentsReport(_reader.Object, _names.Object).ReportCode, Is.EqualTo("documents-expiring"));
        Assert.That(new MissingRequiredDocumentsReport(_reader.Object).ReportCode, Is.EqualTo("documents-missing-required"));
        Assert.That(new DocumentShareActivityReport(_reader.Object, _names.Object).ReportCode, Is.EqualTo("documents-share-activity"));
        Assert.That(new DocumentUploadVolumeReport(_reader.Object).ReportCode, Is.EqualTo("documents-upload-volume"));
    }

    [Test]
    public async Task MissingRequired_ListsOwnersLackingActiveRequiredCategory()
    {
        var transporterId = Guid.NewGuid();
        _reader.Setup(r => r.GetDocumentTypesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new ReportDocumentTypeVm("SOAT", true, true),
            new ReportDocumentTypeVm("RTM", true, true),
            new ReportDocumentTypeVm("Photo", false, true)));
        _reader.Setup(r => r.GetTransporterDocumentComplianceAsync(It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new TransporterDocumentComplianceVm(transporterId, "Fleet A", List("SOAT"))));

        var result = await new MissingRequiredDocumentsReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(1));
        Assert.That(result.Cell(0, "Category"), Is.EqualTo("RTM"));
        Assert.That(result.Cell(0, "OwnerName"), Is.EqualTo("Fleet A"));
        // One batched compliance read — never one call per transporter.
        _reader.Verify(r => r.GetTransporterDocumentComplianceAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task MissingRequired_CategoryComparisonIsCaseInsensitive()
    {
        var transporterId = Guid.NewGuid();
        _reader.Setup(r => r.GetDocumentTypesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new ReportDocumentTypeVm("SOAT", true, true)));
        _reader.Setup(r => r.GetTransporterDocumentComplianceAsync(It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new TransporterDocumentComplianceVm(transporterId, "Fleet A", List("soat"))));

        var result = await new MissingRequiredDocumentsReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(0));
    }

    [Test]
    public async Task MissingRequired_NoRequiredTypes_ReturnsEmpty()
    {
        _reader.Setup(r => r.GetDocumentTypesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new ReportDocumentTypeVm("Photo", false, true)));

        var result = await new MissingRequiredDocumentsReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(0));
        _reader.Verify(r => r.GetTransporterDocumentComplianceAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Expiring_UsesWithinDaysFilter_AndReturnsRows()
    {
        _reader.Setup(r => r.GetExpiringDocumentsAsync(15, It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new ReportDocumentVm("SOAT", "Transporter", "t1", "a.pdf", "Internal", "Active", DateTimeOffset.UtcNow.AddDays(3)),
            new ReportDocumentVm("RTM", "Transporter", "t2", "b.pdf", "Internal", "Active", DateTimeOffset.UtcNow.AddDays(1))));

        var filters = _filters with { Values = FilterValues.Of((FilterNames.WithinDays, "15")) };
        var result = await new ExpiringDocumentsReport(_reader.Object, _names.Object).GetDatasetAsync(filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(2));
        // Sorted by nearest expiry first.
        Assert.That(result.Cell(0, "Category"), Is.EqualTo("RTM"));
    }

    [Test]
    public async Task Expiring_ShowsOwnerNames_AndKeepsTheIdWhenUnknown()
    {
        var unit = Guid.NewGuid();
        var staff = Guid.NewGuid();
        var unknown = Guid.NewGuid();
        _names.Setup(n => n.GetTransporterNamesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string> { [unit] = "AAA000" });
        _names.Setup(n => n.GetStaffNamesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string> { [staff] = "Ana Rojas" });
        _reader.Setup(r => r.GetExpiringDocumentsAsync(30, It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new ReportDocumentVm("SOAT", "Transporter", unit.ToString(), "a.pdf", "Internal", "Active", DateTimeOffset.UtcNow.AddDays(1)),
            new ReportDocumentVm("License", "Staff", staff.ToString(), "b.pdf", "Internal", "Active", DateTimeOffset.UtcNow.AddDays(2)),
            new ReportDocumentVm("RTM", "Transporter", unknown.ToString(), "c.pdf", "Internal", "Active", DateTimeOffset.UtcNow.AddDays(3))));

        var result = await new ExpiringDocumentsReport(_reader.Object, _names.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Cell(0, "OwnerName"), Is.EqualTo("AAA000"));
        Assert.That(result.Cell(1, "OwnerName"), Is.EqualTo("Ana Rojas"));
        Assert.That(result.Cell(2, "OwnerName"), Is.EqualTo(unknown.ToString()));
    }

    [Test]
    public async Task ShareActivity_FiltersToDocumentResourceType()
    {
        _reader.Setup(r => r.GetDocumentSharesByAccountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new ReportShareVm("Document", DocumentId.ToString(), "document.read", "share", DateTimeOffset.UtcNow, null, 3, DateTimeOffset.UtcNow),
            new ReportShareVm("Transporter", "tr-1", "read", "x", DateTimeOffset.UtcNow, null, 9, null)));


        _names.Setup(n => n.GetDocumentNamesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string> { [DocumentId] = "SOAT 2026" });

        var result = await new DocumentShareActivityReport(_reader.Object, _names.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(1));
        Assert.That(result.Cell(0, "SharedDocument"), Is.EqualTo("SOAT 2026"));
    }

    [Test]
    public async Task UploadVolume_GroupsByCategory()
    {
        _reader.Setup(r => r.SearchDocumentsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new ReportDocumentVm("SOAT", "Transporter", "t1", "a", "Internal", "Active", null),
            new ReportDocumentVm("SOAT", "Transporter", "t2", "b", "Internal", "Active", null),
            new ReportDocumentVm("RTM", "Transporter", "t3", "c", "Internal", "Active", null)));

        var result = await new DocumentUploadVolumeReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(2));
        Assert.That(result.Cell(0, "Category"), Is.EqualTo("SOAT"));
        Assert.That(result.Cell(0, "DocumentCount"), Is.EqualTo(2));
    }
}
