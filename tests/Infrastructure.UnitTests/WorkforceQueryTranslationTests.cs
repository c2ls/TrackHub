using System.Net.Sockets;
using Common.Application.Interfaces;
using Moq;
using Npgsql;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;

namespace Infrastructure.UnitTests;

/// <summary>
/// Guards against LINQ that the real PostgreSQL provider cannot translate.
/// <para>
/// Every other test in this suite runs on EF InMemory, which evaluates ANY expression client-side and
/// therefore cannot fail on an untranslatable query. That blind spot shipped a real bug: the workforce
/// readers ordered the PROJECTED record struct (<c>Project(query).OrderBy(x =&gt; x.StartsAt)</c>), which
/// Npgsql rejects with "could not be translated" — green unit tests, "Unexpected Execution Error" in
/// production.
/// </para>
/// <para>
/// These tests use the REAL Npgsql provider against an unreachable host. Translation happens before any
/// connection is attempted, so an untranslatable query fails with <see cref="InvalidOperationException"/>
/// ("could not be translated") while a correct one gets as far as a connection error. Asserting on
/// WHICH failure occurs verifies translation without needing a database.
/// </para>
/// </summary>
[TestFixture]
public class WorkforceQueryTranslationTests
{
    // Port 1 is never listening; Timeout=1 keeps the connection failure fast.
    private const string UnreachableConnection = "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=1;Command Timeout=1";

    private static ApplicationDbContext NewNpgsqlContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(UnreachableConnection)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options);

    // AccountId matches the request, so RequireAccountAccess short-circuits without touching the DB and
    // the reader gets all the way to translating its own query.
    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(x => x.AccountId).Returns(accountId);
        principal.SetupGet(x => x.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        return principal.Object;
    }

    private static async Task AssertTranslatesAsync(Func<Task> query)
    {
        try
        {
            await query();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("could not be translated", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail($"The query cannot be translated to SQL and would fail at runtime:{Environment.NewLine}{ex.Message}");
            return;
        }
        catch (Exception ex) when (IsConnectionFailure(ex))
        {
            // The ONLY outcome that proves translation succeeded: EF built the SQL, then the
            // provider tried to open a socket to a host that is not listening.
            return;
        }
        catch (Exception ex)
        {
            // A guard clause, an ArgumentException or an EF wording change used to land in a bare
            // `catch (Exception) { }` and score a free pass, so the test asserted nothing at all.
            Assert.Fail(
                $"The query failed before it reached the database, so nothing was translated: " +
                $"{ex.GetType().FullName}: {ex.Message}");
            return;
        }

        // Returning normally means the reader never opened a connection — an early return or a null
        // guard short-circuited it, and no SQL was ever generated.
        Assert.Fail("The query completed without attempting a connection, so no SQL was translated.");
    }

    /// <summary>
    /// A failure to reach 127.0.0.1:1. Npgsql wraps the socket error, and the shape of the wrapping
    /// differs by platform and version, so the whole inner chain is inspected.
    /// </summary>
    private static bool IsConnectionFailure(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is NpgsqlException or SocketException or TimeoutException)
                return true;
        }

        return false;
    }

    [Test]
    public async Task GetDriverQualifications_TranslatesToSql()
    {
        using var context = NewNpgsqlContext();
        var accountId = Guid.NewGuid();
        var reader = new DriverQualificationReader(context, Principal(accountId));

        await AssertTranslatesAsync(() => reader.GetDriverQualificationsAsync(accountId, null, null, 0, 50, CancellationToken.None));
    }

    [Test]
    public async Task GetDriverQualifications_WithEveryFilter_TranslatesToSql()
    {
        using var context = NewNpgsqlContext();
        var accountId = Guid.NewGuid();
        var reader = new DriverQualificationReader(context, Principal(accountId));

        await AssertTranslatesAsync(() => reader.GetDriverQualificationsAsync(accountId, Guid.NewGuid(), 30, 10, 100, CancellationToken.None));
    }

    [Test]
    public async Task GetDriverAssignmentHistory_TranslatesToSql()
    {
        using var context = NewNpgsqlContext();
        var accountId = Guid.NewGuid();
        var reader = new DriverAssignmentReader(context, Principal(accountId));

        await AssertTranslatesAsync(() => reader.GetDriverAssignmentHistoryAsync(accountId, null, null, null, null, 0, 50, CancellationToken.None));
    }

    [Test]
    public async Task GetDriverAssignmentHistory_WithEveryFilter_TranslatesToSql()
    {
        using var context = NewNpgsqlContext();
        var accountId = Guid.NewGuid();
        var reader = new DriverAssignmentReader(context, Principal(accountId));

        await AssertTranslatesAsync(() => reader.GetDriverAssignmentHistoryAsync(
            accountId, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow, 0, 50, CancellationToken.None));
    }

    [Test]
    public async Task DriverReader_WorkforceQueries_TranslateToSql()
    {
        using var context = NewNpgsqlContext();
        var accountId = Guid.NewGuid();
        var reader = new DriverReader(context, Principal(accountId));
        var driverId = Guid.NewGuid();

        await AssertTranslatesAsync(() => reader.GetDriverAssignmentsAsync(driverId, CancellationToken.None));
        await AssertTranslatesAsync(() => reader.ValidateDriverAssignmentAsync(driverId, "Transporter", Guid.NewGuid().ToString(), CancellationToken.None));
        await AssertTranslatesAsync(() => reader.GetMyDriverProfileAsync(driverId, CancellationToken.None));
    }
}
