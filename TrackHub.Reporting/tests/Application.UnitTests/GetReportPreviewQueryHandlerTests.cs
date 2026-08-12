using Moq;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Application.Report.Queries.Get;
using TrackHub.Reporting.Domain.Exceptions;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Options;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests.Reports;

[TestFixture]
public class GetReportPreviewQueryHandlerTests
{
    private Mock<IReportFactory> _factory = null!;
    private Mock<IUser> _user = null!;
    private Mock<IReportCatalogReader> _catalog = null!;
    private Mock<IAccountFeatureReader> _features = null!;
    private Mock<IReport> _report = null!;
    private ReportingLimitsOptions _limits = null!;
    private Guid _accountId;
    private const string Code = "preview-report";

    [SetUp]
    public void SetUp()
    {
        _factory = new Mock<IReportFactory>();
        _user = new Mock<IUser>();
        _catalog = new Mock<IReportCatalogReader>();
        _features = new Mock<IAccountFeatureReader>();
        _report = new Mock<IReport>();
        _limits = new ReportingLimitsOptions { PreviewRows = 5 };
        _accountId = Guid.NewGuid();

        _user.Setup(u => u.AccountId).Returns(_accountId);
        _user.Setup(u => u.Role).Returns(Roles.User);
        _factory.Setup(f => f.GetReport(Code)).Returns(_report.Object);
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReportMetadataVm(Code, null, "Gps", null, false, false, 1, true));
    }

    private GetReportPreviewQueryHandler Handler()
        => new(_factory.Object, _user.Object, _catalog.Object, _features.Object, _limits);

    private void SetupDataset(int rows)
        => _report.Setup(r => r.GetDatasetAsync(It.IsAny<FilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReportDataset
            {
                Title = "T",
                Columns = [new ReportColumn("Status", typeof(string)), new ReportColumn("Speed", typeof(double))],
                Rows = [.. Enumerable.Range(0, rows).Select(i => new object?[] { $"s{i}", (double)i })]
            });

    private static FilterDto Filters() => new() { Name = "T", Language = "en" };

    [Test]
    public async Task Preview_CapsRows_AndReportsTotalsAndTruncation()
    {
        SetupDataset(12);

        var result = await Handler().Handle(new GetReportPreviewQuery(Code, Filters()), CancellationToken.None);

        Assert.That(result.Rows, Has.Count.EqualTo(5));
        Assert.That(result.TotalRows, Is.EqualTo(12));
        Assert.That(result.Truncated, Is.True);
        Assert.That(result.Columns.Select(c => c.Name), Is.EqualTo(new[] { "Status", "Speed" }));
    }

    [Test]
    public async Task Preview_UnderCap_NotTruncated()
    {
        SetupDataset(3);

        var result = await Handler().Handle(new GetReportPreviewQuery(Code, Filters()), CancellationToken.None);

        Assert.That(result.Rows, Has.Count.EqualTo(3));
        Assert.That(result.TotalRows, Is.EqualTo(3));
        Assert.That(result.Truncated, Is.False);
    }

    [Test]
    public async Task Preview_ResolvesLocalizedHeaders()
    {
        SetupDataset(1);

        var result = await Handler().Handle(new GetReportPreviewQuery(Code, Filters()), CancellationToken.None);

        // "Status" resolves via resx; every column resolves to a non-empty header.
        Assert.That(result.Columns.First(c => c.Name == "Status").Header, Is.EqualTo("Status"));
        Assert.That(result.Columns.First(c => c.Name == "Speed").Header, Is.Not.Empty);
    }

    [Test]
    public void Preview_UnknownCode_ThrowsReportNotFound()
    {
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportMetadataVm?)null);

        Assert.ThrowsAsync<ReportNotFoundException>(() =>
            Handler().Handle(new GetReportPreviewQuery(Code, Filters()), CancellationToken.None));
    }

    [Test]
    public void Preview_ManagerOnly_PlainUser_ThrowsAccessDenied()
    {
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReportMetadataVm(Code, null, "Gps", null, true, false, 1, true));

        Assert.ThrowsAsync<ReportAccessDeniedException>(() =>
            Handler().Handle(new GetReportPreviewQuery(Code, Filters()), CancellationToken.None));
    }

    [Test]
    public void Preview_FeatureDisabled_Propagates()
    {
        _catalog.Setup(c => c.GetReportByCodeAsync(Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReportMetadataVm(Code, null, "Gps", FeatureKeys.Documents, false, false, 1, true));
        _features.Setup(f => f.EnsureFeatureEnabledAsync(_accountId, FeatureKeys.Documents, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FeatureDisabledException(FeatureKeys.Documents));

        Assert.ThrowsAsync<FeatureDisabledException>(() =>
            Handler().Handle(new GetReportPreviewQuery(Code, Filters()), CancellationToken.None));
    }
}
