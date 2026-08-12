// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using Common.Application.Interfaces;
using Common.Domain.Constants;
using Moq;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class ReportReaderTests
{
    private const short BasicType = 1;

    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(
        Guid? accountId,
        string? role = Roles.User,
        PrincipalType principalType = PrincipalType.User,
        Guid? userId = null)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.PrincipalType).Returns(principalType);
        principal.SetupGet(p => p.UserId).Returns(userId);
        principal.SetupGet(p => p.Role).Returns(role);
        return principal.Object;
    }

    private static Report Global(string code, int sortOrder = 10, string category = "Operations", bool active = true)
        => new(code, code, BasicType, active, category, null, false, false, sortOrder);

    private static Report Gated(string code, string featureKey, int sortOrder = 10, string category = "Gps")
        => new(code, code, BasicType, true, category, featureKey, false, false, sortOrder);

    private static Report ManagerOnly(string code, int sortOrder = 10)
        => new(code, code, BasicType, true, "Administration", null, true, false, sortOrder);

    private static ReportReader Reader(ApplicationDbContext context, ICurrentPrincipal principal)
        => new(context as IApplicationDbContext, principal);

    [Test]
    public async Task GetReportsAsync_GlobalReport_VisibleToPlainUser()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetReportsAsync_GlobalReport_VisibleToPlainUser));
        await context.Reports.AddAsync(Global("LiveReport"));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Reader(context, Principal(accountId)).GetReportsAsync(CancellationToken.None);

        Assert.That(result.Select(r => r.Code), Is.EquivalentTo(new[] { "LiveReport" }));
    }

    [Test]
    public async Task GetReportsAsync_FeatureGated_HiddenWithoutFeatureRow()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetReportsAsync_FeatureGated_HiddenWithoutFeatureRow));
        await context.Reports.AddAsync(Gated("gps.sync-statistics", FeatureKeys.GpsIntegration));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Reader(context, Principal(accountId)).GetReportsAsync(CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetReportsAsync_FeatureGated_VisibleWithEnabledFeature()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetReportsAsync_FeatureGated_VisibleWithEnabledFeature));
        await context.Reports.AddAsync(Gated("gps.sync-statistics", FeatureKeys.GpsIntegration));
        await context.AccountFeatures.AddAsync(new AccountFeature(accountId, FeatureKeys.GpsIntegration, true, "standard", "manual", null, null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Reader(context, Principal(accountId)).GetReportsAsync(CancellationToken.None);

        Assert.That(result.Select(r => r.Code), Is.EquivalentTo(new[] { "gps.sync-statistics" }));
    }

    [Test]
    public async Task GetReportsAsync_FeatureGated_HiddenWhenEffectiveToExpired()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetReportsAsync_FeatureGated_HiddenWhenEffectiveToExpired));
        await context.Reports.AddAsync(Gated("gps.sync-statistics", FeatureKeys.GpsIntegration));
        await context.AccountFeatures.AddAsync(new AccountFeature(accountId, FeatureKeys.GpsIntegration, true, "standard", "manual",
            DateTimeOffset.UtcNow.AddDays(-10), DateTimeOffset.UtcNow.AddDays(-1), null));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Reader(context, Principal(accountId)).GetReportsAsync(CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetReportsAsync_ManagerOnly_HiddenForUser()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetReportsAsync_ManagerOnly_HiddenForUser));
        await context.Reports.AddAsync(ManagerOnly("accounts-by-status"));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Reader(context, Principal(accountId, Roles.User)).GetReportsAsync(CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [TestCase("Manager")]
    [TestCase("Administrator")]
    public async Task GetReportsAsync_ManagerOnly_VisibleForPrivilegedRole(string role)
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetReportsAsync_ManagerOnly_VisibleForPrivilegedRole) + role);
        await context.Reports.AddAsync(ManagerOnly("accounts-by-status"));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Reader(context, Principal(accountId, role)).GetReportsAsync(CancellationToken.None);

        Assert.That(result.Select(r => r.Code), Is.EquivalentTo(new[] { "accounts-by-status" }));
    }

    [Test]
    public async Task GetReportsAsync_GlobalServiceClient_SeesAllActive()
    {
        await using var context = NewContext(nameof(GetReportsAsync_GlobalServiceClient_SeesAllActive));
        await context.Reports.AddAsync(Global("LiveReport"));
        await context.Reports.AddAsync(Gated("gps.sync-statistics", FeatureKeys.GpsIntegration));
        await context.Reports.AddAsync(ManagerOnly("accounts-by-status"));
        await context.Reports.AddAsync(Global("Inactive", active: false));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Reader(context, Principal(null, principalType: PrincipalType.ServiceClient)).GetReportsAsync(CancellationToken.None);

        Assert.That(result.Select(r => r.Code), Is.EquivalentTo(new[] { "LiveReport", "gps.sync-statistics", "accounts-by-status" }));
    }

    [Test]
    public async Task GetReportsAsync_InactiveRow_NeverReturned()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetReportsAsync_InactiveRow_NeverReturned));
        await context.Reports.AddAsync(Global("Inactive", active: false));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Reader(context, Principal(accountId)).GetReportsAsync(CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetReportsAsync_UserWithoutAccount_OnlyGlobalNonManagerOnly()
    {
        await using var context = NewContext(nameof(GetReportsAsync_UserWithoutAccount_OnlyGlobalNonManagerOnly));
        await context.Reports.AddAsync(Global("LiveReport"));
        await context.Reports.AddAsync(Gated("gps.sync-statistics", FeatureKeys.GpsIntegration));
        await context.Reports.AddAsync(ManagerOnly("accounts-by-status"));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Reader(context, Principal(null)).GetReportsAsync(CancellationToken.None);

        Assert.That(result.Select(r => r.Code), Is.EquivalentTo(new[] { "LiveReport" }));
    }

    [Test]
    public async Task GetReportsAsync_OrdersByCategoryThenSortOrderThenCode()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetReportsAsync_OrdersByCategoryThenSortOrderThenCode));
        await context.Reports.AddAsync(Global("Bravo", sortOrder: 20, category: "Operations"));
        await context.Reports.AddAsync(Global("Alpha", sortOrder: 10, category: "Operations"));
        await context.Reports.AddAsync(Global("Admin", sortOrder: 10, category: "Administration"));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Reader(context, Principal(accountId, Roles.Administrator)).GetReportsAsync(CancellationToken.None);

        Assert.That(result.Select(r => r.Code).ToArray(), Is.EqualTo(new[] { "Admin", "Alpha", "Bravo" }));
    }

    [Test]
    public async Task GetReportByCodeAsync_ReturnsInactiveRow()
    {
        await using var context = NewContext(nameof(GetReportByCodeAsync_ReturnsInactiveRow));
        await context.Reports.AddAsync(Global("Inactive", active: false));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Reader(context, Principal(Guid.NewGuid())).GetReportByCodeAsync("Inactive", CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.Active, Is.False);
    }

    [Test]
    public async Task GetReportByCodeAsync_UnknownCode_ReturnsNull()
    {
        await using var context = NewContext(nameof(GetReportByCodeAsync_UnknownCode_ReturnsNull));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Reader(context, Principal(Guid.NewGuid())).GetReportByCodeAsync("missing", CancellationToken.None);

        Assert.That(result, Is.Null);
    }
}
