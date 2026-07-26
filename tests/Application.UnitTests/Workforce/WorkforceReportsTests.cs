using System.Globalization;
using System.Resources;
using Moq;
using TrackHub.Reporting.Application.Report.Factory.Workforce;
using TrackHub.Reporting.Domain.Helpers;
using TrackHub.Reporting.Application.UnitTests;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests.Workforce;

[TestFixture]
public class WorkforceReportsTests
{
    private Mock<IWorkforceReportReader> _reader = null!;
    private FilterDto _filters;

    [SetUp]
    public void SetUp()
    {
        _reader = new Mock<IWorkforceReportReader>();
        _reader.Setup(r => r.EnsureWorkforceFeatureAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _filters = new FilterDto { Language = "en", Name = "Test" };
    }

    private static IReadOnlyCollection<T> List<T>(params T[] items) => items;

    // The reports derive "today" the same way — from DateTimeOffset.UtcNow's calendar date.
    private static DateOnly Today
    {
        get
        {
            var now = DateTimeOffset.UtcNow;
            return new DateOnly(now.Year, now.Month, now.Day);
        }
    }

    [Test]
    public void Codes_MatchSpecConstants()
    {
        Assert.That(new DriverRegistryReport(_reader.Object).ReportCode, Is.EqualTo("workforce-driver-registry"));
        Assert.That(new QualificationExpirationsReport(_reader.Object).ReportCode, Is.EqualTo("workforce-qualification-expirations"));
        Assert.That(new AssignmentHistoryReport(_reader.Object).ReportCode, Is.EqualTo("workforce-assignment-history"));
    }

    // ---- Driver registry ----

    [Test]
    public async Task Registry_OrdersByNameAndProjectsNullsToEmptyStrings()
    {
        var transporterId = Guid.NewGuid();
        _reader.Setup(r => r.GetDriversAsync(It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new ReportDriverVm(Guid.NewGuid(), "Zulema", null, null, null, true, null, null, null, null),
            new ReportDriverVm(Guid.NewGuid(), "ana", "E-1", "CC", "123", true, "E-1", "LIC-9", new DateOnly(2027, 3, 4), transporterId)));

        var result = await new DriverRegistryReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(2));
        // Case-insensitive alphabetical order.
        Assert.That(result.Cell(0, "DriverName"), Is.EqualTo("ana"));
        Assert.That(result.Cell(0, "DocumentNumber"), Is.EqualTo("123"));
        Assert.That(result.Cell(0, "LicenseExpiresAt"), Is.EqualTo(new DateTimeOffset(2027, 3, 4, 0, 0, 0, TimeSpan.Zero)));
        Assert.That(result.Cell(0, "DefaultTransporterId"), Is.EqualTo(transporterId.ToString()));
        // Nulls become empty strings, never null cells.
        Assert.That(result.Cell(1, "Phone"), Is.EqualTo(string.Empty));
        Assert.That(result.Cell(1, "LicenseNumber"), Is.EqualTo(string.Empty));
        Assert.That(result.Cell(1, "LicenseExpiresAt"), Is.Null);
        Assert.That(result.Cell(1, "DefaultTransporterId"), Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task Registry_ColumnOrderMatchesRowVmDeclaration()
    {
        _reader.Setup(r => r.GetDriversAsync(It.IsAny<CancellationToken>())).ReturnsAsync(List<ReportDriverVm>());

        var result = await new DriverRegistryReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Columns.Select(c => c.PropertyName), Is.EqualTo(new[]
        {
            "DriverName", "EmployeeCode", "DocumentType", "DocumentNumber", "Phone",
            "LicenseNumber", "LicenseExpiresAt", "DefaultTransporterId", "Active"
        }));
    }

    [Test]
    public async Task Registry_TakesNoFilters()
    {
        _reader.Setup(r => r.GetDriversAsync(It.IsAny<CancellationToken>())).ReturnsAsync(List<ReportDriverVm>());

        await new DriverRegistryReport(_reader.Object).GetDatasetAsync(
            _filters with { NumericFilter1 = 5, StringFilter1 = Guid.NewGuid().ToString() }, CancellationToken.None);

        _reader.Verify(r => r.GetDriversAsync(It.IsAny<CancellationToken>()), Times.Once);
        _reader.Verify(r => r.EnsureWorkforceFeatureAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---- Qualification expirations ----

    [Test]
    public async Task Expirations_DefaultsToThirtyDayWindow()
    {
        _reader.Setup(r => r.GetDriverQualificationsAsync(null, 30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportDriverQualificationVm>())
            .Verifiable();

        await new QualificationExpirationsReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        _reader.Verify();
    }

    [Test]
    public async Task Expirations_UsesNumericFilter1AsWindow()
    {
        _reader.Setup(r => r.GetDriverQualificationsAsync(null, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportDriverQualificationVm>())
            .Verifiable();

        await new QualificationExpirationsReport(_reader.Object).GetDatasetAsync(
            _filters with { NumericFilter1 = 7 }, CancellationToken.None);

        _reader.Verify();
    }

    [Test]
    public async Task Expirations_SortsByNearestExpiryFirst_NullsLast()
    {
        _reader.Setup(r => r.GetDriverQualificationsAsync(It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new ReportDriverQualificationVm(Guid.NewGuid(), Guid.NewGuid(), "Carlos", "License", "C2", "L-1", null, Today.AddDays(20), "RUNT", "Valid"),
            new ReportDriverQualificationVm(Guid.NewGuid(), Guid.NewGuid(), "Nunca", "Other", null, null, null, null, null, "Valid"),
            new ReportDriverQualificationVm(Guid.NewGuid(), Guid.NewGuid(), "Ana", "MedicalExam", null, "M-1", null, Today.AddDays(3), null, "Valid")));

        var result = await new QualificationExpirationsReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Column("DriverName"), Is.EqualTo(new object?[] { "Ana", "Carlos", "Nunca" }));
        Assert.That(result.Cell(2, "DaysRemaining"), Is.Null);
    }

    [Test]
    public async Task Expirations_ComputesDaysRemaining_NegativeWhenExpired()
    {
        _reader.Setup(r => r.GetDriverQualificationsAsync(It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new ReportDriverQualificationVm(Guid.NewGuid(), Guid.NewGuid(), "Expired", "License", "C1", "L-0", null, Today.AddDays(-5), null, "Expired"),
            new ReportDriverQualificationVm(Guid.NewGuid(), Guid.NewGuid(), "Soon", "License", "C1", "L-2", null, Today.AddDays(10), null, "Valid")));

        var result = await new QualificationExpirationsReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Cell(0, "DaysRemaining"), Is.EqualTo(-5));
        Assert.That(result.Cell(1, "DaysRemaining"), Is.EqualTo(10));
        Assert.That(result.Cell(0, "Status"), Is.EqualTo("Expired"));
        Assert.That(result.Cell(0, "LicenseCategory"), Is.EqualTo("C1"));
        Assert.That(result.Cell(0, "IssuedAt"), Is.Null);
    }

    [Test]
    public async Task Expirations_ColumnOrderMatchesRowVmDeclaration()
    {
        _reader.Setup(r => r.GetDriverQualificationsAsync(It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportDriverQualificationVm>());

        var result = await new QualificationExpirationsReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Columns.Select(c => c.PropertyName), Is.EqualTo(new[]
        {
            "DriverName", "QualificationType", "LicenseCategory", "Number",
            "IssuingAuthority", "IssuedAt", "ExpiresAt", "DaysRemaining", "Status"
        }));
    }

    // ---- Assignment history ----

    // StringFilter1 is the portal's transporter picker slot (filtersData.ts registers
    // ['transporter','from','to'] for this code) — the report must read it as the transporter id.
    [Test]
    public async Task AssignmentHistory_ReadsStringFilter1AsTransporterId()
    {
        var transporterId = Guid.NewGuid();
        var from = DateTimeOffset.UtcNow.AddDays(-30);
        var to = DateTimeOffset.UtcNow;

        _reader.Setup(r => r.GetDriverAssignmentHistoryAsync(null, transporterId, from, to, It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportDriverAssignmentVm>())
            .Verifiable();

        var filters = _filters with
        {
            StringFilter1 = transporterId.ToString(),
            DateTimeFilter1 = from,
            DateTimeFilter2 = to
        };
        await new AssignmentHistoryReport(_reader.Object).GetDatasetAsync(filters, CancellationToken.None);

        _reader.Verify();
    }

    [Test]
    public async Task AssignmentHistory_IgnoresUnparseableAndEmptyIdFilters()
    {
        _reader.Setup(r => r.GetDriverAssignmentHistoryAsync(null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportDriverAssignmentVm>())
            .Verifiable();

        var filters = _filters with { StringFilter1 = "not-a-guid" };
        await new AssignmentHistoryReport(_reader.Object).GetDatasetAsync(filters, CancellationToken.None);

        _reader.Verify();

        _reader.Invocations.Clear();
        await new AssignmentHistoryReport(_reader.Object).GetDatasetAsync(
            _filters with { StringFilter1 = Guid.Empty.ToString() }, CancellationToken.None);

        _reader.Verify();
    }

    // stringFilter2 is the portal's free-text "device" slot and is not part of this report's spec;
    // whatever leaks into it must never become a filter.
    [Test]
    public async Task AssignmentHistory_IgnoresStringFilter2()
    {
        _reader.Setup(r => r.GetDriverAssignmentHistoryAsync(null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportDriverAssignmentVm>())
            .Verifiable();

        await new AssignmentHistoryReport(_reader.Object).GetDatasetAsync(
            _filters with { StringFilter2 = Guid.NewGuid().ToString() }, CancellationToken.None);

        _reader.Verify();
    }

    [Test]
    public async Task AssignmentHistory_OrdersMostRecentFirst_AndComputesDuration()
    {
        var older = DateTimeOffset.UtcNow.AddDays(-40);
        var newer = DateTimeOffset.UtcNow.AddDays(-10);

        _reader.Setup(r => r.GetDriverAssignmentHistoryAsync(
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(
                new ReportDriverAssignmentVm(Guid.NewGuid(), "Ana", Guid.NewGuid(), "Truck 1", older, older.AddDays(5), "Regular", "Ended", "user:1"),
                new ReportDriverAssignmentVm(Guid.NewGuid(), "Beto", Guid.NewGuid(), "Truck 2", newer, null, "Temporary", "Active", "user:2")));

        var result = await new AssignmentHistoryReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(2));
        Assert.That(result.Cell(0, "DriverName"), Is.EqualTo("Beto"));
        Assert.That(result.Cell(0, "TransporterName"), Is.EqualTo("Truck 2"));
        Assert.That(result.Cell(0, "AssignmentType"), Is.EqualTo("Temporary"));
        Assert.That(result.Cell(0, "EndsAt"), Is.Null);
        // Open assignment: elapsed to now.
        Assert.That(result.Cell(0, "DurationDays"), Is.EqualTo(10));
        // Closed assignment: bounded by EndsAt.
        Assert.That(result.Cell(1, "DurationDays"), Is.EqualTo(5));
        Assert.That(result.Cell(1, "CreatedByPrincipal"), Is.EqualTo("user:1"));
    }

    [Test]
    public async Task AssignmentHistory_ColumnOrderMatchesRowVmDeclaration()
    {
        _reader.Setup(r => r.GetDriverAssignmentHistoryAsync(
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportDriverAssignmentVm>());

        var result = await new AssignmentHistoryReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Columns.Select(c => c.PropertyName), Is.EqualTo(new[]
        {
            "DriverName", "TransporterName", "AssignmentType", "Status",
            "StartsAt", "EndsAt", "DurationDays", "CreatedByPrincipal"
        }));
    }

    [Test]
    public void AllReports_EnforceTheWorkforceFeatureBeforeReading()
    {
        _reader.Setup(r => r.EnsureWorkforceFeatureAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => new DriverRegistryReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None));
        Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => new QualificationExpirationsReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None));
        Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => new AssignmentHistoryReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None));

        _reader.Verify(r => r.GetDriversAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // The three workforce row VMs, one per report — the header checks below are per-report because
    // a duplicate header only matters between columns a user sees side by side.
    private static readonly Type[] WorkforceRowVms =
        [typeof(DriverRegistryRowVm), typeof(QualificationExpirationRowVm), typeof(DriverAssignmentHistoryRowVm)];

    // A resx key that is absent resolves to the raw key, so "resolved != key" is the presence probe.
    // Some English values legitimately equal the key ("Phone", "Number", "Status", "Active"), which
    // would make a genuine miss invisible — so presence is asserted directly against the resource
    // set instead of inferred from the resolved text, and no column is exempted from any check.
    // The generated Resources class is internal to the Domain assembly, so bind the ResourceManager
    // by base name against that assembly instead of by type.
    private static readonly ResourceManager Resources =
        new("TrackHub.Reporting.Domain.Resources.Resources", typeof(ReportHeaderResolver).Assembly);

    private static IEnumerable<string> ColumnKeys(Type rowVm)
        => rowVm.GetProperties().Select(p => p.Name);

    // Every row-VM property name is a resx header key; an unmapped key silently falls back to the
    // raw property name in the export, so guard both shipped cultures.
    [TestCase("en")]
    [TestCase("es")]
    public void EveryWorkforceColumnHasALocalizedHeader(string language)
    {
        var culture = new CultureInfo(language);
        // tryParents:false — the culture's own resx must carry the key. Neutral fallback would hide
        // exactly the kind of per-file inconsistency this test exists to catch.
        var set = Resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
        Assert.That(set, Is.Not.Null, $"No resource set for '{language}'.");

        foreach (var name in WorkforceRowVms.SelectMany(ColumnKeys).Distinct())
        {
            var localized = set!.GetString(name);
            Assert.That(localized, Is.Not.Null, $"Missing '{language}' resx entry for column '{name}'.");
            Assert.That(localized, Is.Not.Empty, $"Empty '{language}' resx value for column '{name}'.");
            Assert.That(ReportHeaderResolver.Resolve(name, culture), Is.EqualTo(localized));
        }
    }

    // A header that is still the raw PascalCase property name is an untranslated column leaking into
    // the export; "Number"/"Status"/"Phone"/"Active" happen to be correct English words, so they are
    // allowed to equal their key in `en` only — never in `es`.
    private static readonly string[] EnglishWordsIdenticalToTheirKey = ["Phone", "Number", "Status", "Active"];

    [TestCase("en")]
    [TestCase("es")]
    public void NoWorkforceColumnHeaderIsTheRawPropertyName(string language)
    {
        var culture = new CultureInfo(language);

        foreach (var name in WorkforceRowVms.SelectMany(ColumnKeys).Distinct())
        {
            if (language == "en" && EnglishWordsIdenticalToTheirKey.Contains(name))
            {
                continue;
            }

            Assert.That(ReportHeaderResolver.Resolve(name, culture), Is.Not.EqualTo(name),
                $"Column '{name}' resolves to its own key in '{language}'.");
        }
    }

    // The regression guard for the "Qualification Type" / "Type" collision: two columns of the SAME
    // report resolving to one header string are indistinguishable to whoever reads the export, even
    // though both keys exist and both are translated.
    [TestCase("en")]
    [TestCase("es")]
    public void NoTwoColumnsOfAReportShareAHeader(string language)
    {
        var culture = new CultureInfo(language);

        foreach (var rowVm in WorkforceRowVms)
        {
            var byHeader = ColumnKeys(rowVm)
                .GroupBy(name => ReportHeaderResolver.Resolve(name, culture), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => $"'{g.Key}' ← {string.Join(", ", g)}")
                .ToList();

            Assert.That(byHeader, Is.Empty,
                $"{rowVm.Name} has duplicate '{language}' headers: {string.Join(" | ", byHeader)}");
        }
    }

    // The check that actually catches the shipped defect: `Category` resolved to "Type"/"Tipo" and
    // sat next to `QualificationType` ("Qualification Type"/"Tipo de Habilitación"). The two strings
    // differ, so an equality check passes — but one header wholly containing another in the same
    // report is exactly what makes a column meaningless to whoever reads the export.
    [TestCase("en")]
    [TestCase("es")]
    public void NoColumnHeaderIsContainedInAnotherHeaderOfTheSameReport(string language)
    {
        var culture = new CultureInfo(language);

        foreach (var rowVm in WorkforceRowVms)
        {
            var headers = ColumnKeys(rowVm)
                .Select(name => (Key: name, Header: ReportHeaderResolver.Resolve(name, culture)))
                .ToList();

            foreach (var (key, header) in headers)
            {
                var swallowedBy = headers
                    .Where(other => other.Key != key
                        && other.Header.Contains(header, StringComparison.CurrentCultureIgnoreCase))
                    .Select(other => $"{other.Key} (\"{other.Header}\")")
                    .ToList();

                Assert.That(swallowedBy, Is.Empty,
                    $"{rowVm.Name}: '{language}' header \"{header}\" for column '{key}' is indistinguishable from {string.Join(", ", swallowedBy)}.");
            }
        }
    }
}
