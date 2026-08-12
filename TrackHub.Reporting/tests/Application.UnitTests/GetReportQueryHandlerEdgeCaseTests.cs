using System.Globalization;
using Moq;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Application.Report.Queries.Get;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Helpers;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Options;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests.Reports;

[TestFixture]
public class GetReportQueryHandlerEdgeCaseTests
{
    private sealed class StubExcelHelper : IExcelHelper
    {
        public byte[] Result = [];
        public byte[] Export(ReportDataset dataset, CultureInfo culture) => Result;
    }

    private Mock<IReportFactory> _factory = null!;
    private Mock<IUser> _user = null!;
    private Mock<IReportAuditWriter> _audit = null!;
    private Mock<IReportCatalogReader> _catalog = null!;
    private Mock<IAccountFeatureReader> _features = null!;
    private Mock<IReport> _report = null!;
    private Mock<IPdfReportBuilder> _pdf = null!;
    private Mock<IReportBrandingReader> _branding = null!;
    private StubExcelHelper _excel = null!;
    private Guid _accountId;
    private const string Code = "edge-report";

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
        _pdf = new Mock<IPdfReportBuilder>();
        _excel = new StubExcelHelper();
        _accountId = Guid.NewGuid();

        _user.Setup(u => u.AccountId).Returns(_accountId);
        _user.Setup(u => u.PrincipalType).Returns(PrincipalType.User);
        _user.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _user.Setup(u => u.Role).Returns(Roles.User);
        _factory.Setup(f => f.GetReport(Code)).Returns(_report.Object);
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReportMetadataVm(Code, null, "Operations", null, false, false, 1, true));
    }

    private GetReportQueryHandler Handler()
        => new(_factory.Object, _user.Object, _audit.Object, _catalog.Object,
            _features.Object, _branding.Object, _excel, _pdf.Object, new ReportingLimitsOptions());

    private static ReportDataset Dataset(int rows) => new()
    {
        Title = "T",
        Columns = [new ReportColumn("A", typeof(string))],
        Rows = [.. Enumerable.Range(0, rows).Select(i => new object?[] { i.ToString() })]
    };

    private static FilterDto Filters() => new() { Name = "T", Language = "en" };

    [Test]
    public async Task EmptyDataset_ReturnsFile_WithZeroRowCount()
    {
        _excel.Result = [];
        _report.Setup(r => r.GetDatasetAsync(It.IsAny<FilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Dataset(0));

        var result = await Handler().Handle(new GetReportQuery(Code, Filters()), CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(0));
        Assert.That(result.Content, Is.Empty);
    }

    [Test]
    public void ReportThrows_PropagatesException()
    {
        _report.Setup(r => r.GetDatasetAsync(It.IsAny<FilterDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("source unavailable"));

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            Handler().Handle(new GetReportQuery(Code, Filters()), CancellationToken.None));
    }

    [Test]
    public async Task RowCount_FlowsFromDatasetToAuditAndResult()
    {
        _excel.Result = [9, 9];
        _report.Setup(r => r.GetDatasetAsync(It.IsAny<FilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Dataset(42));

        var result = await Handler().Handle(new GetReportQuery(Code, Filters()), CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(42));
        _audit.Verify(a => a.RecordReportExportAsync(_accountId, It.IsAny<string>(), It.IsAny<string>(), Code,
            It.IsAny<string>(), 42, "xlsx", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
