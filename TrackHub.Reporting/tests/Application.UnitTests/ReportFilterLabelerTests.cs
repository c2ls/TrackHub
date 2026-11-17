using System.Globalization;
using Moq;
using TrackHub.Reporting.Application.Report.Filters;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models.Manager;

namespace TrackHub.Reporting.Application.UnitTests;

[TestFixture]
public class ReportFilterLabelerTests
{
    private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es");

    private static ReportFilterLabeler Labeler(string? transporterName = null, params ManagerOperatorVm[] operators)
    {
        var names = new Mock<IReportNameReader>();
        names.Setup(n => n.GetTransporterNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(transporterName);
        names.Setup(n => n.GetOperatorNamesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(operators.ToDictionary(o => o.OperatorId, o => o.Name));
        return new ReportFilterLabeler(names.Object);
    }

    private static KeyValuePair<string, object> Filter(string key, object value) => new(key, value);

    [Test]
    public async Task ReplacesIdsWithNames_AndTranslatesStatus()
    {
        var operatorId = Guid.NewGuid();
        var from = DateTimeOffset.UnixEpoch;

        var labeled = await Labeler("AAA000", new ManagerOperatorVm(operatorId, "Traccar 1", true)).LabelAsync(
            [Filter("FilterTransporterId", Guid.NewGuid().ToString()), Filter("FilterOperatorId", operatorId.ToString()),
             Filter("FilterStatus", "SUSPENDED"), Filter("FilterFrom", from), Filter("FilterWithinDays", "30")],
            Spanish, CancellationToken.None);

        Assert.That(labeled.Select(f => f.Value), Is.EqualTo(new object[] { "AAA000", "Traccar 1", "Suspendida", from, "30" }));
    }

    [Test]
    public async Task KeepsTheIdWhenNoNameIsFound()
    {
        var transporterId = Guid.NewGuid().ToString();
        var operatorId = Guid.NewGuid().ToString();

        var labeled = await Labeler().LabelAsync(
            [Filter("FilterTransporterId", transporterId), Filter("FilterOperatorId", operatorId), Filter("FilterStatus", "Unheard")],
            Spanish, CancellationToken.None);

        Assert.That(labeled.Select(f => f.Value), Is.EqualTo(new object[] { transporterId, operatorId, "Unheard" }));
    }
}
