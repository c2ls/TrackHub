// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using Common.Domain.Enums;
using TrackHub.Manager.Application.Reports.Commands.Update;
using TrackHub.Manager.Application.Reports.Queries.GetAll;
using TrackHub.Manager.Application.Reports.Queries.GetByCode;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Reports;

[TestFixture]
public class ReportQueryTests
{
    private Mock<IReportReader> _readerMock;
    private Mock<IReportWriter> _writerMock;

    [SetUp]
    public void Setup()
    {
        _readerMock = new Mock<IReportReader>();
        _writerMock = new Mock<IReportWriter>();
    }

    private static ReportVm Vm(string code, string category = "Operations", bool active = true)
        => new(Guid.NewGuid(), code, code, ReportType.Basic, (short)ReportType.Basic, active, category, null, false, false, 10);

    [Test]
    public async Task GetReports_ReturnsReaderResult()
    {
        var reports = new[] { Vm("LiveReport"), Vm("GeofenceEvents") };
        _readerMock.Setup(r => r.GetReportsAsync(CancellationToken.None)).ReturnsAsync(reports);
        var handler = new GetReportsQueryHandler(_readerMock.Object);

        var result = await handler.Handle(new GetReportsQuery(), CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(2));
        _readerMock.Verify(r => r.GetReportsAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task GetReportByCode_KnownCode_ReturnsVm()
    {
        var vm = Vm("LiveReport");
        _readerMock.Setup(r => r.GetReportByCodeAsync("LiveReport", CancellationToken.None)).ReturnsAsync(vm);
        var handler = new GetReportByCodeQueryHandler(_readerMock.Object);

        var result = await handler.Handle(new GetReportByCodeQuery("LiveReport"), CancellationToken.None);

        Assert.That(result, Is.EqualTo(vm));
    }

    [Test]
    public async Task GetReportByCode_UnknownCode_ReturnsNull()
    {
        _readerMock.Setup(r => r.GetReportByCodeAsync("nope", CancellationToken.None)).ReturnsAsync((ReportVm?)null);
        var handler = new GetReportByCodeQueryHandler(_readerMock.Object);

        var result = await handler.Handle(new GetReportByCodeQuery("nope"), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateReport_PassesDtoToWriter()
    {
        var dto = new UpdateReportDto(Guid.NewGuid(), "desc", (short)ReportType.Basic, true, "Gps", "gps.integration", true, true, 30);
        var handler = new UpdateReportCommandHandler(_writerMock.Object);

        await handler.Handle(new UpdateReportCommand(dto), CancellationToken.None);

        _writerMock.Verify(w => w.UpdateReportAsync(
            It.Is<UpdateReportDto>(d => d.Category == "Gps" && d.RequiredFeatureKey == "gps.integration" && d.ManagerOnly && d.SupportsPdf && d.SortOrder == 30),
            CancellationToken.None), Times.Once);
    }

    [Test]
    public void UpdateReportValidator_EmptyCategory_Fails()
    {
        var validator = new UpdateReportValidator();
        var dto = new UpdateReportDto(Guid.NewGuid(), "desc", (short)ReportType.Basic, true, "", null, false, false, 0);

        var result = validator.Validate(new UpdateReportCommand(dto));

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void GetReportByCodeValidator_EmptyCode_Fails()
    {
        var validator = new GetReportByCodeValidator();

        var result = validator.Validate(new GetReportByCodeQuery(string.Empty));

        Assert.That(result.IsValid, Is.False);
    }
}
