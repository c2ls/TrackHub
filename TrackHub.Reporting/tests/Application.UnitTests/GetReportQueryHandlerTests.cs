using System.Globalization;
using Moq;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Application.Report.Queries.Get;
using TrackHub.Reporting.Domain.Exceptions;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Helpers;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Options;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests.Reports;

[TestFixture]
public class GetReportQueryHandlerTests
{
    private sealed class StubExcelHelper : IExcelHelper
    {
        public static readonly byte[] Bytes = [1, 2, 3];
        public ReportDataset? Last;
        public byte[] Export(ReportDataset dataset, CultureInfo culture)
        {
            Last = dataset;
            return Bytes;
        }
    }

    private sealed class StubPdfBuilder : IPdfReportBuilder
    {
        public static readonly byte[] Bytes = [0x25, 0x50, 0x44, 0x46]; // %PDF
        public ReportDataset? Last;
        public byte[] Build(ReportDataset dataset, CultureInfo culture)
        {
            Last = dataset;
            return Bytes;
        }
    }

    private Mock<IReportFactory> _factory = null!;
    private Mock<IUser> _user = null!;
    private Mock<IReportAuditWriter> _audit = null!;
    private Mock<IReportCatalogReader> _catalog = null!;
    private Mock<IAccountFeatureReader> _features = null!;
    private Mock<IReportBrandingReader> _branding = null!;
    private Mock<IReport> _report = null!;
    private StubExcelHelper _excel = null!;
    private StubPdfBuilder _pdf = null!;
    private ReportingLimitsOptions _limits = null!;
    private Guid _accountId;
    private const string Code = "some-report";

    [SetUp]
    public void SetUp()
    {
        _factory = new Mock<IReportFactory>();
        _user = new Mock<IUser>();
        _audit = new Mock<IReportAuditWriter>();
        _catalog = new Mock<IReportCatalogReader>();
        _features = new Mock<IAccountFeatureReader>();
        _branding = new Mock<IReportBrandingReader>();
        _report = new Mock<IReport>();
        _excel = new StubExcelHelper();
        _pdf = new StubPdfBuilder();
        _limits = new ReportingLimitsOptions();
        _accountId = Guid.NewGuid();

        _user.Setup(u => u.AccountId).Returns(_accountId);
        _user.Setup(u => u.PrincipalType).Returns(PrincipalType.User);
        _user.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _user.Setup(u => u.CorrelationId).Returns("corr");
        _user.Setup(u => u.Role).Returns(Roles.User);

        _factory.Setup(f => f.GetReport(Code)).Returns(_report.Object);
        _report.Setup(r => r.GetDatasetAsync(It.IsAny<FilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Dataset(3));
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Metadata());
    }

    private static ReportMetadataVm Metadata(
        bool active = true, string? feature = null, bool managerOnly = false, bool supportsPdf = true)
        => new(Code, "desc", "Gps", feature, managerOnly, supportsPdf, 1, active);

    private static ReportDataset Dataset(int rows) => new()
    {
        Title = "Title",
        Columns = [new ReportColumn("A", typeof(string))],
        Rows = [.. Enumerable.Range(0, rows).Select(i => new object?[] { $"r{i}" })]
    };

    private GetReportQueryHandler BuildHandler()
        => new(_factory.Object, _user.Object, _audit.Object, _catalog.Object,
            _features.Object, _branding.Object, _excel, _pdf, _limits);

    private static FilterDto Filters(string language = "en") => new() { Name = "Title", Language = language };

    [Test]
    public async Task Xlsx_ReturnsExcelBytes_AndAuditsWithFormatAndRowCount()
    {
        var result = await BuildHandler().Handle(new GetReportQuery(Code, Filters()), CancellationToken.None);

        Assert.That(result.Content, Is.EqualTo(StubExcelHelper.Bytes));
        Assert.That(result.ContentType, Is.EqualTo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        Assert.That(result.FileExtension, Is.EqualTo(".xlsx"));
        Assert.That(result.RowCount, Is.EqualTo(3));
        _audit.Verify(a => a.RecordReportExportAsync(_accountId, "User", It.IsAny<string>(), Code,
            It.IsAny<string>(), 3, "xlsx", "corr", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Pdf_ReturnsPdfBytes_AndAuditsPdf()
    {
        var result = await BuildHandler().Handle(new GetReportQuery(Code, Filters(), "pdf"), CancellationToken.None);

        Assert.That(result.Content, Is.EqualTo(StubPdfBuilder.Bytes));
        Assert.That(result.ContentType, Is.EqualTo("application/pdf"));
        Assert.That(result.FileExtension, Is.EqualTo(".pdf"));
        _audit.Verify(a => a.RecordReportExportAsync(_accountId, "User", It.IsAny<string>(), Code,
            It.IsAny<string>(), 3, "pdf", "corr", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Pdf_AppliesAccountBrandingToDataset()
    {
        var logo = new byte[] { 9, 8, 7 };
        _branding.Setup(b => b.GetBrandingAsync(_accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReportBrandingVm("Acme Fleet", logo));

        await BuildHandler().Handle(new GetReportQuery(Code, Filters(), "pdf"), CancellationToken.None);

        Assert.That(_pdf.Last, Is.Not.Null);
        Assert.That(_pdf.Last!.AccountName, Is.EqualTo("Acme Fleet"));
        Assert.That(_pdf.Last.LogoImage, Is.EqualTo(logo));
    }

    [Test]
    public async Task Xlsx_DoesNotFetchBranding()
    {
        await BuildHandler().Handle(new GetReportQuery(Code, Filters()), CancellationToken.None);

        _branding.Verify(b => b.GetBrandingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Pdf_BrandingNull_ExportsWithoutBranding()
    {
        _branding.Setup(b => b.GetBrandingAsync(_accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportBrandingVm?)null);

        var result = await BuildHandler().Handle(new GetReportQuery(Code, Filters(), "pdf"), CancellationToken.None);

        Assert.That(result.Content, Is.EqualTo(StubPdfBuilder.Bytes));
        Assert.That(_pdf.Last!.AccountName, Is.Null);
        Assert.That(_pdf.Last.LogoImage, Is.Null);
    }

    [Test]
    public void UnknownCode_ThrowsReportNotFound()
    {
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportMetadataVm?)null);

        Assert.ThrowsAsync<ReportNotFoundException>(() =>
            BuildHandler().Handle(new GetReportQuery(Code, Filters()), CancellationToken.None));
    }

    [Test]
    public void InactiveReport_ThrowsReportNotFound()
    {
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Metadata(active: false));

        Assert.ThrowsAsync<ReportNotFoundException>(() =>
            BuildHandler().Handle(new GetReportQuery(Code, Filters()), CancellationToken.None));
    }

    [Test]
    public void RequiredFeature_Enforced_FeatureDisabledPropagates()
    {
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Metadata(feature: FeatureKeys.Geofencing));
        _features.Setup(f => f.EnsureFeatureEnabledAsync(_accountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FeatureDisabledException(FeatureKeys.Geofencing));

        Assert.ThrowsAsync<FeatureDisabledException>(() =>
            BuildHandler().Handle(new GetReportQuery(Code, Filters()), CancellationToken.None));
        _features.Verify(f => f.EnsureFeatureEnabledAsync(_accountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void ManagerOnly_PlainUser_ThrowsAccessDenied()
    {
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Metadata(managerOnly: true));
        _user.Setup(u => u.Role).Returns(Roles.User);

        Assert.ThrowsAsync<ReportAccessDeniedException>(() =>
            BuildHandler().Handle(new GetReportQuery(Code, Filters()), CancellationToken.None));
    }

    [Test]
    public async Task ManagerOnly_ManagerRole_Succeeds()
    {
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Metadata(managerOnly: true));
        _user.Setup(u => u.Role).Returns(Roles.Manager);

        var result = await BuildHandler().Handle(new GetReportQuery(Code, Filters()), CancellationToken.None);
        Assert.That(result.RowCount, Is.EqualTo(3));
    }

    [Test]
    public async Task ManagerOnly_AdministratorRole_Succeeds()
    {
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Metadata(managerOnly: true));
        _user.Setup(u => u.Role).Returns(Roles.Administrator);

        var result = await BuildHandler().Handle(new GetReportQuery(Code, Filters()), CancellationToken.None);
        Assert.That(result.RowCount, Is.EqualTo(3));
    }

    [Test]
    public void PdfOnExcelOnlyReport_ThrowsUnsupportedFormat()
    {
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Metadata(supportsPdf: false));

        Assert.ThrowsAsync<UnsupportedReportFormatException>(() =>
            BuildHandler().Handle(new GetReportQuery(Code, Filters(), "pdf"), CancellationToken.None));
    }

    [Test]
    public void PdfOverMaxPdfRows_ThrowsLimitExceeded()
    {
        _limits.MaxPdfRows = 2;
        _report.Setup(r => r.GetDatasetAsync(It.IsAny<FilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Dataset(3));

        var ex = Assert.ThrowsAsync<ReportLimitExceededException>(() =>
            BuildHandler().Handle(new GetReportQuery(Code, Filters(), "pdf"), CancellationToken.None));
        Assert.That(ex!.MaxRows, Is.EqualTo(2));
    }

    [Test]
    public void UnsupportedFormat_ThrowsUnsupportedFormat()
    {
        Assert.ThrowsAsync<UnsupportedReportFormatException>(() =>
            BuildHandler().Handle(new GetReportQuery(Code, Filters(), "csv"), CancellationToken.None));
    }

    [Test]
    public void MissingAccount_ThrowsUnauthorized()
    {
        _user.Setup(u => u.AccountId).Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            BuildHandler().Handle(new GetReportQuery(Code, Filters()), CancellationToken.None));
    }

    [Test]
    public async Task BadLanguage_FallsBackWithoutThrowing()
    {
        var result = await BuildHandler().Handle(
            new GetReportQuery(Code, Filters("not-a-real-language")), CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(3));
    }

    [Test]
    public async Task AbsentFormat_DefaultsToXlsx()
    {
        // Simulates the backward-compatible portal payload (no format field).
        var query = new GetReportQuery(Code, Filters());
        Assert.That(query.Format, Is.EqualTo("xlsx"));

        var result = await BuildHandler().Handle(query, CancellationToken.None);
        Assert.That(result.ContentType, Does.Contain("spreadsheetml"));
    }
}
