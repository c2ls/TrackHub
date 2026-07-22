// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using TrackHub.TripManagement.Domain.Constants;
using TrackHub.TripManagement.Domain.Records;
using TrackHub.TripManagement.Infrastructure.TripDB.Entities;
using TrackHub.TripManagement.Infrastructure.TripDB.Readers;
using TrackHub.TripManagement.Infrastructure.TripDB.Writers;

namespace Infrastructure.UnitTests;

/// <summary>
/// Acceptance 21's temporal tariffs — <c>TollCatalogWriter.CreateTariffAsync</c>, which had no test.
/// <para>
/// The whole reason tariffs carry an <c>EffectiveFrom</c>/<c>EffectiveTo</c> window instead of a
/// single mutable price is REPRODUCIBILITY: a trip planned in January must still price out at
/// January's rate when someone re-runs the report in July. That guarantee is one line deep — the
/// open row is CLOSED at <c>newFrom - 1 day</c>, never overwritten — and an "optimisation" that
/// updated the existing row in place would leave every test of the estimate green while silently
/// rewriting history for every trip already taken.
/// </para>
/// <para>
/// The second half is error attribution. <c>SaveUniqueAsync</c> takes the error code as an ARGUMENT
/// because it used to be a constant: every duplicate in this writer was reported as
/// <c>TOLL_OVERLAPPING_TARIFF</c>, so an administrator whose CSV had a repeated station name got a
/// row-level error naming a completely different entity, which is unactionable.
/// </para>
/// </summary>
[TestFixture]
public class TollTariffTemporalTests
{
    private static readonly Guid StationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private const string ClassII = "II";

    private static readonly DateOnly January = new(2026, 1, 1);
    private static readonly DateOnly July = new(2026, 7, 1);

    private static async Task<WriterTestContext> SeededAsync()
    {
        var context = WriterTestContext.Create();
        context.TollStations.Add(new TollStation
        {
            TollStationId = StationId,
            Name = "Peaje Chusaca",
            Code = "CHU",
            Point = WriterTestData.Point(4.55, -74.25),
            Active = true,
        });
        context.TollVehicleClasses.Add(new TollVehicleClass { Code = ClassII, Name = "Clase II", Active = true });
        await context.SaveChangesAsync(CancellationToken.None);
        return context;
    }

    private static TollCatalogWriter Writer(WriterTestContext context)
    {
        var user = new Mock<IUser>();
        user.SetupGet(u => u.Id).Returns(Guid.NewGuid().ToString());
        user.SetupGet(u => u.PrincipalType).Returns(PrincipalType.User);

        // The REAL reader, over the same context: the overlap decision is the reader's half-open
        // window predicate, and stubbing it would leave the interesting half untested.
        return new TollCatalogWriter(context, new TollCatalogReader(context), user.Object);
    }

    private static TollTariffDto Tariff(decimal amount, DateOnly from, DateOnly? to = null, string currency = "COP")
        => new(StationId, ClassII, amount, currency, from, to);

    // ----- The price change --------------------------------------------------------------------

    [Test]
    public async Task APriceChange_ClosesTheOpenRowTheDayBeforeInsteadOfOverwritingIt()
    {
        // The single assertion acceptance 21 stands on. Overwriting would leave one row, the new
        // price, and no way to reprice a January trip.
        using var context = await SeededAsync();
        var writer = Writer(context);

        await writer.CreateTariffAsync(Tariff(12_500m, January), CancellationToken.None);
        await writer.CreateTariffAsync(Tariff(14_200m, July), CancellationToken.None);

        var tariffs = await context.TollTariffs.AsNoTracking()
            .OrderBy(t => t.EffectiveFrom)
            .ToListAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(tariffs, Has.Count.EqualTo(2), "the old price was overwritten instead of closed");

            Assert.That(tariffs[0].Amount, Is.EqualTo(12_500m), "the historical price must be untouched");
            Assert.That(tariffs[0].EffectiveFrom, Is.EqualTo(January));
            Assert.That(tariffs[0].EffectiveTo, Is.EqualTo(new DateOnly(2026, 6, 30)),
                "the open row closes the day BEFORE the new one opens — no gap, no overlap");

            Assert.That(tariffs[1].Amount, Is.EqualTo(14_200m));
            Assert.That(tariffs[1].EffectiveFrom, Is.EqualTo(July));
            Assert.That(tariffs[1].EffectiveTo, Is.Null, "the new price is the open row");
        });
    }

    [Test]
    public async Task AThirdPriceChange_ClosesOnlyTheCurrentlyOpenRow()
    {
        // Three windows, chained. A closing rule that reached back over already-closed rows would
        // shred the history it exists to preserve.
        using var context = await SeededAsync();
        var writer = Writer(context);

        await writer.CreateTariffAsync(Tariff(12_500m, January), CancellationToken.None);
        await writer.CreateTariffAsync(Tariff(14_200m, July), CancellationToken.None);
        await writer.CreateTariffAsync(Tariff(15_000m, new DateOnly(2026, 10, 1)), CancellationToken.None);

        var tariffs = await context.TollTariffs.AsNoTracking()
            .OrderBy(t => t.EffectiveFrom)
            .ToListAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(tariffs, Has.Count.EqualTo(3));
            Assert.That(tariffs[0].EffectiveTo, Is.EqualTo(new DateOnly(2026, 6, 30)));
            Assert.That(tariffs[1].EffectiveTo, Is.EqualTo(new DateOnly(2026, 9, 30)));
            Assert.That(tariffs[2].EffectiveTo, Is.Null);
        });
    }

    [Test]
    public async Task ADifferentVehicleClass_DoesNotCloseAnotherClassesOpenRow()
    {
        // Windows are per (station, class). Closing across classes would silently expire the price
        // for every other class at the station the moment one class was repriced.
        using var context = await SeededAsync();
        context.TollVehicleClasses.Add(new TollVehicleClass { Code = "III", Name = "Clase III", Active = true });
        await context.SaveChangesAsync(CancellationToken.None);
        var writer = Writer(context);

        await writer.CreateTariffAsync(Tariff(12_500m, January), CancellationToken.None);
        await writer.CreateTariffAsync(
            new TollTariffDto(StationId, "III", 18_000m, "COP", July, null), CancellationToken.None);

        var classII = await context.TollTariffs.AsNoTracking()
            .FirstAsync(t => t.TollVehicleClassCode == ClassII, CancellationToken.None);

        Assert.That(classII.EffectiveTo, Is.Null, "class II's window was closed by a class III price change");
    }

    // ----- Rejections --------------------------------------------------------------------------

    [Test]
    public async Task AGenuinelyOverlappingWindow_IsAConflict()
    {
        // A window landing inside an already-CLOSED historical window is not a price change, it is
        // an ambiguity: two rows would answer "what did this cost on 1 March?".
        //
        // Note the deliberate absence of an OPEN row. With one present the backdate guard fires
        // first and the test would pass even with the overlap check disabled entirely — it would
        // assert the right code for the wrong reason, which is how this whole module got to ~2000
        // green tests while broken.
        using var context = await SeededAsync();
        var writer = Writer(context);

        await writer.CreateTariffAsync(
            Tariff(12_500m, January, new DateOnly(2026, 6, 30)), CancellationToken.None);

        var conflict = Assert.ThrowsAsync<ConflictException>(() => writer.CreateTariffAsync(
            Tariff(13_000m, new DateOnly(2026, 3, 1), new DateOnly(2026, 4, 1)), CancellationToken.None));

        Assert.That(conflict!.Code, Is.EqualTo(TripErrorCodes.OverlappingTariff));
    }

    [Test]
    public async Task ABackdatedEffectiveFrom_IsRejected()
    {
        // Closing the open row at newFrom - 1 day would give it an EffectiveTo BEFORE its own
        // EffectiveFrom — an inverted window that matches nothing, so the station would silently
        // stop pricing for that class entirely.
        using var context = await SeededAsync();
        var writer = Writer(context);

        await writer.CreateTariffAsync(Tariff(14_200m, July), CancellationToken.None);

        var conflict = Assert.ThrowsAsync<ConflictException>(
            () => writer.CreateTariffAsync(Tariff(12_500m, new DateOnly(2026, 6, 1)), CancellationToken.None));

        Assert.That(conflict!.Code, Is.EqualTo(TripErrorCodes.OverlappingTariff));

        var tariffs = await context.TollTariffs.AsNoTracking().ToListAsync(CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(tariffs, Has.Count.EqualTo(1), "the rejected insert must not have landed");
            Assert.That(tariffs[0].EffectiveTo, Is.Null, "the open row must not have been closed by a rejected change");
        });
    }

    [Test]
    public async Task AnEffectiveFromOnTheSameDayAsTheOpenRow_IsRejected()
    {
        // The boundary. Same-day would close the open row at the day before its own start.
        using var context = await SeededAsync();
        var writer = Writer(context);

        await writer.CreateTariffAsync(Tariff(14_200m, July), CancellationToken.None);

        var conflict = Assert.ThrowsAsync<ConflictException>(
            () => writer.CreateTariffAsync(Tariff(15_000m, July), CancellationToken.None));

        Assert.That(conflict!.Code, Is.EqualTo(TripErrorCodes.OverlappingTariff));
    }

    [Test]
    public async Task ASucceedingWindowThatStartsTheDayAfterAClosedOne_IsAccepted()
    {
        // The complement of the overlap rule: adjacent windows are the normal shape and must not
        // be mistaken for a conflict, or a catalog could only ever be priced once.
        using var context = await SeededAsync();
        var writer = Writer(context);

        await writer.CreateTariffAsync(
            Tariff(12_500m, January, new DateOnly(2026, 6, 30)), CancellationToken.None);
        await writer.CreateTariffAsync(Tariff(14_200m, July), CancellationToken.None);

        var tariffs = await context.TollTariffs.AsNoTracking().ToListAsync(CancellationToken.None);
        Assert.That(tariffs, Has.Count.EqualTo(2));
    }

    // ----- Error attribution: the right code per index ------------------------------------------

    [Test]
    public async Task ADuplicateStation_IsReportedAsADuplicateStationNotAnOverlappingTariff()
    {
        // The defect SaveUniqueAsync's code argument exists for. In the §7.6 row-level import
        // report, "overlapping tariff" against a repeated station name sends the administrator
        // looking at the wrong column of the wrong entity.
        using var context = await SeededAsync();
        var writer = Writer(context);
        context.FailNextSaveOn("ux_toll_stations_name_code");

        var conflict = Assert.ThrowsAsync<ConflictException>(() => writer.CreateStationAsync(
            new TollStationDto("Peaje Chusaca", "CHU", 4.55, -74.25, "CO", null, null, null, null, null),
            CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(conflict!.Code, Is.EqualTo(TripErrorCodes.DuplicateTollStation));
            Assert.That(conflict!.Code, Is.Not.EqualTo(TripErrorCodes.OverlappingTariff));
        });
    }

    [Test]
    public async Task ADuplicateVehicleClass_IsReportedAsADuplicateVehicleClass()
    {
        using var context = await SeededAsync();
        var writer = Writer(context);
        context.FailNextSaveOn("ux_toll_vehicle_classes_code");

        // A distinct code: the point under test is which ERROR CODE the violation is translated
        // into, and the violation is injected, so re-adding the seeded key would only trip the
        // in-memory provider's own tracking rules before the writer got a chance to translate.
        var conflict = Assert.ThrowsAsync<ConflictException>(() => writer.CreateVehicleClassAsync(
            new TollVehicleClassDto("IV", "Clase IV", null, 4), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(conflict!.Code, Is.EqualTo(TripErrorCodes.DuplicateTollVehicleClass));
            Assert.That(conflict!.Code, Is.Not.EqualTo(TripErrorCodes.OverlappingTariff));
        });
    }

    [Test]
    public async Task ATariffViolatingTheOpenRowIndex_IsStillReportedAsAnOverlappingTariff()
    {
        // The partial unique index on (station, class) WHERE effective_to IS NULL is the last-line
        // race guard: two concurrent price changes both pass the read-side overlap check.
        using var context = await SeededAsync();
        var writer = Writer(context);
        context.FailNextSaveOn("ux_toll_tariffs_station_class_open");

        var conflict = Assert.ThrowsAsync<ConflictException>(
            () => writer.CreateTariffAsync(Tariff(12_500m, January), CancellationToken.None));

        Assert.That(conflict!.Code, Is.EqualTo(TripErrorCodes.OverlappingTariff));
    }

    [Test]
    public async Task AfterADuplicate_TheSameContextIsStillUsable()
    {
        // The same request-scoped-context trap the POD writer had: the failed Added entries must
        // leave the change tracker, or the NEXT row of an import replays the dead insert and one
        // bad row cascades into failing the whole remainder of the batch.
        using var context = await SeededAsync();
        var writer = Writer(context);
        context.FailNextSaveOn("ux_toll_stations_name_code");

        Assert.ThrowsAsync<ConflictException>(() => writer.CreateStationAsync(
            new TollStationDto("Peaje Chusaca", "CHU", 4.55, -74.25, "CO", null, null, null, null, null),
            CancellationToken.None));

        var created = await writer.CreateStationAsync(
            new TollStationDto("Peaje Boqueron", "BOQ", 4.45, -74.30, "CO", null, null, null, null, null),
            CancellationToken.None);

        var stations = await context.TollStations.AsNoTracking().ToListAsync(CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(created.Name, Is.EqualTo("Peaje Boqueron"));
            Assert.That(stations, Has.Count.EqualTo(2), "the rejected insert was replayed or the genuine one was lost");
        });
    }
}
