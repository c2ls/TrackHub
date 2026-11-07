/**
 * Trips (feature `trip-management`): the dispatch board, trip planning and the
 * detail workspace.
 *
 * The lifecycle is AUTOMATIC (spec 11a): a trip starts when its unit leaves the
 * origin zone and completes at its last stop. The manual verbs behind
 * "Override" are exception handling, and a browser cannot fabricate the GPS that
 * drives the real transitions — so what is exercised here is planning, the
 * workspace, and the overrides that a `Created` trip legitimately offers.
 *
 * Every trip a test creates is released by that test: a `Created` trip holds its
 * unit's arming slot, and a leftover silently absorbs the arming of every trip
 * created after it (spec 11b).
 */

import { test, expect, unique, uniqueName, optional } from '../fixtures';
import { FormDialog, ConfirmDialog, pickOption } from '../pages/dialogs';
import { boardRow, planTrip, plannerStop, plannerStopNamed, releaseAfterwards } from '../pages/trips';

test.describe('trips', () => {
  test('the dispatch board renders its filters, actions and pagination', async ({
    shell,
    page,
    t,
  }) => {
    await shell.open('tripManager');

    for (const name of [
      t('trips.filterStatus'),
      t('trips.exceptions.label'),
      t('trips.filterTransporter'),
      t('trips.filterStaff'),
    ]) {
      await expect(page.getByRole('combobox', { name })).toBeVisible({ timeout: 60_000 });
    }
    await expect(page.getByRole('button', { name: t('trips.newTrip') })).toBeVisible();
    await expect(page.getByRole('button', { name: t('trips.import.action') })).toBeVisible();
    await expect(page.getByText(t('trips.board'))).toBeVisible();
    await expect(
      page.getByText(/\d+\s*[–-]\s*\d+/).or(page.getByText(t('trips.noTrips'))).first()
    ).toBeVisible();
  });

  test('the new-trip dialog refuses a trip with no code, unit, start or places', async ({
    shell,
    page,
    t,
  }) => {
    await shell.open('tripManager');
    await page.getByRole('button', { name: t('trips.newTrip') }).click();

    const form = new FormDialog(page, t);
    await form.waitOpen();
    await form.save();

    await expect(form.root).toBeVisible();
    await expect(form.error('code')).toHaveText(t('validation.required', { field: 'code' }));
    await form.cancel();
  });

  test('a dispatcher plans a trip, sees it on the board and deletes it', async ({
    shell,
    page,
    t,
    api,
    cleanup,
  }) => {
    const code = `e2e-trip-${unique()}`;
    await shell.open('tripManager');

    releaseAfterwards(api, cleanup, code);

    await planTrip(page, t, code);

    await expect(boardRow(page, code)).toBeVisible({ timeout: 60_000 });
    // The workspace opens on the new trip, in the state a new trip has.
    await expect(page.getByText(t('trips.statuses.Created'), { exact: true }).first()).toBeVisible();

    // A `Created` trip with no history can be deleted outright.
    await page.getByRole('button', { name: t('trips.actions.delete') }).click();
    await new ConfirmDialog(page, t).confirm();
    await expect(boardRow(page, code)).toHaveCount(0, { timeout: 45_000 });
  });

  test('the trip workspace offers every panel of a planned trip', async ({
    shell,
    page,
    t,
    api,
    cleanup,
  }) => {
    const code = `e2e-trip-${unique()}`;
    await shell.open('tripManager');
    releaseAfterwards(api, cleanup, code);

    await planTrip(page, t, code);

    for (const tab of ['route', 'assignment', 'stops', 'tolls', 'pod', 'timeline', 'replay']) {
      const label = t(`trips.workspace.${tab}`);
      await page.getByRole('tab', { name: label }).click();
      await expect(page.getByRole('tab', { name: label })).toHaveAttribute('aria-selected', 'true');
    }

    // The stops the plan created are listed.
    await page.getByRole('tab', { name: t('trips.workspace.stops') }).click();
    await expect(
      page.getByText(t('tripStops.empty')).or(page.getByRole('columnheader').first()).first()
    ).toBeVisible();
  });

  test('the override panel offers only the verbs a Created trip can take', async ({
    shell,
    page,
    t,
    api,
    cleanup,
  }) => {
    const code = `e2e-trip-${unique()}`;
    await shell.open('tripManager');
    releaseAfterwards(api, cleanup, code);

    await planTrip(page, t, code);

    await page.getByRole('button', { name: t('trips.override.action') }).click();
    await expect(page.getByText(t('trips.override.hint'))).toBeVisible();

    // From `Created`: start, "already in transit" and cancel are offered; the
    // running-only verbs are not.
    await expect(page.getByRole('button', { name: t('trips.actions.start') })).toBeVisible();
    await expect(page.getByRole('button', { name: t('trips.inTransit.action') })).toBeVisible();
    await expect(page.getByRole('button', { name: t('trips.actions.cancel') })).toBeVisible();
    await expect(page.getByRole('button', { name: t('trips.actions.pause') })).toHaveCount(0);
    await expect(page.getByRole('button', { name: t('trips.actions.resume') })).toHaveCount(0);
    await expect(page.getByRole('button', { name: t('trips.actions.abort') })).toHaveCount(0);

    // Cancelling asks for a reason and moves the trip out of `Created`.
    await page.getByRole('button', { name: t('trips.actions.cancel') }).click();
    const reason = new FormDialog(page, t);
    await reason.waitOpen();
    await reason.field('reason').fill('cancelled by the e2e suite');
    await reason.saveAndClose();

    await expect(
      page.getByText(t('trips.statuses.Cancelled'), { exact: true }).first()
    ).toBeVisible({ timeout: 45_000 });
  });

  test('a dispatcher adds, edits, reorders and removes stops on a planned trip', async ({
    shell,
    page,
    t,
    api,
    cleanup,
  }) => {
    const code = `e2e-trip-${unique()}`;
    await shell.open('tripManager');
    releaseAfterwards(api, cleanup, code);
    await planTrip(page, t, code);

    // Planning left the origin and one destination; the added stop becomes the
    // last of the list.
    await expect(plannerStop(page, 1)).toBeVisible({ timeout: 60_000 });
    const planned = await plannerStop(page, 1).innerText();

    const stopName = `e2e-stop-${unique()}`;
    await page.getByRole('button', { name: t('tripStops.add') }).click();
    const stop = new FormDialog(page, t);
    await stop.waitOpen();
    // The dialog says outright that there is no address search — a stop is
    // placed from the map, a point of interest or a geofence, and nowhere else.
    await expect(stop.root.getByText(t('tripStops.placement.noSearch'))).toBeVisible();
    await expect(stop.root.getByText(t('tripStops.placement.none'))).toBeVisible();

    // Placed from a geofence, so arrival is judged against the zone's real shape.
    await stop.selectFirst('geofencePick');
    await expect(stop.root.getByText(/^Chosen point: /)).toBeVisible();
    await stop.field('name').fill(stopName);
    await stop.field('city').fill('Bogota');
    await stop.saveAndClose();

    const added = plannerStopNamed(page, stopName);
    await expect(added).toBeVisible({ timeout: 45_000 });

    // Editing loads the stop back, and the change sticks across a reopen.
    await added.getByRole('button', { name: t('generic.edit') }).click();
    const edit = new FormDialog(page, t);
    await edit.waitOpen();
    await expect(edit.field('name')).toHaveValue(stopName);
    await edit.field('arrivalRadiusMeters').fill('450');
    await edit.saveAndClose();

    await expect(added).toBeVisible({ timeout: 45_000 });
    await added.getByRole('button', { name: t('generic.edit') }).click();
    await edit.waitOpen();
    await expect(edit.field('arrivalRadiusMeters')).toHaveValue('450');
    await edit.cancel();

    // Reordering renumbers the list: the new stop takes the first slot.
    await added.getByRole('button', { name: t('tripStops.actions.moveUp') }).click();
    await expect(plannerStop(page, 1)).toContainText(stopName, { timeout: 45_000 });
    await expect(plannerStop(page, 2)).not.toContainText(stopName);

    // Removing it puts the trip back to what planning created.
    await plannerStop(page, 1)
      .getByRole('button', { name: t('tripStops.actions.remove') })
      .click();
    await expect(plannerStopNamed(page, stopName)).toHaveCount(0, {
      timeout: 45_000,
    });
    await expect(plannerStop(page, 1)).toContainText(planned.split('\n')[1].trim());
  });

  test('a dispatcher registers a delivery and records its outcome, after which only a pending one can be deleted', async ({
    shell,
    page,
    t,
    api,
    cleanup,
  }) => {
    // Deliveries are captured from THIS screen on purpose: there is no staff
    // app yet, so a dispatcher taking details over the radio has to be able to
    // keep the trip's record complete on their own.
    const code = `e2e-trip-${unique()}`;
    const client = `e2e-client-${unique()}`;
    await shell.open('tripManager');
    releaseAfterwards(api, cleanup, code);
    await planTrip(page, t, code);

    await page.getByRole('tab', { name: t('trips.workspace.stops') }).click();
    const stopRow = page.locator('[data-testid^="row-"]').last();
    await expect(stopRow).toBeVisible({ timeout: 45_000 });
    await stopRow.getByRole('button', { name: t('trips.deliveries.add'), exact: true }).click();

    const delivery = new FormDialog(page, t);
    await delivery.waitOpen();
    // A delivery belongs to the stop it was created on, and the dialog says so.
    await expect(delivery.root.getByText(/^Stop: /)).toBeVisible();
    await delivery.save();
    await expect(delivery.error('clientName')).toBeVisible();

    await delivery.field('clientName').fill(client);
    await delivery.field('reference').fill('E2E-REF-1');
    await delivery.field('productsSummary').fill('2 pallets');
    await delivery.saveAndClose();

    const deliveryRow = () =>
      page.locator('[data-testid^="row-"]').filter({ hasText: client }).first();
    await expect(deliveryRow()).toBeVisible({ timeout: 45_000 });
    await expect(deliveryRow()).toContainText(t('trips.deliveries.statuses.Pending'));

    // The outcome is its own verb: what was promised and what happened are
    // recorded apart.
    await deliveryRow().getByRole('button', { name: t('trips.deliveries.outcome') }).click();
    const outcome = new FormDialog(page, t);
    await outcome.waitOpen();
    await outcome.select('deliveryStatus', t('trips.deliveries.statuses.Rejected'));
    await outcome.field('outcomeObservations').fill('refused at the gate - e2e');
    await outcome.saveAndClose();

    await expect(deliveryRow()).toContainText(t('trips.deliveries.statuses.Rejected'), {
      timeout: 45_000,
    });

    // A recorded outcome is final: only the outcome verb stays.
    await expect(deliveryRow().getByRole('button', { name: t('generic.delete') })).toHaveCount(0);
    await expect(deliveryRow().getByRole('button', { name: t('generic.edit') })).toHaveCount(0);

    const pending = `e2e-client-${unique()}`;
    const addDelivery = page.getByRole('button', { name: t('trips.deliveries.add'), exact: true });
    await page.locator('[data-testid^="row-"]').filter({ has: addDelivery }).last().getByRole('button', { name: t('trips.deliveries.add'), exact: true }).click();
    const second = new FormDialog(page, t);
    await second.waitOpen();
    await second.field('clientName').fill(pending);
    await second.saveAndClose();
    const pendingRow = () => page.locator('[data-testid^="row-"]').filter({ hasText: pending }).first();
    await expect(pendingRow()).toBeVisible({ timeout: 45_000 });

    await pendingRow().getByRole('button', { name: t('generic.delete') }).click();
    const confirm = new ConfirmDialog(page, t);
    await expect(confirm.root.getByText(t('trips.deliveries.deleteMessage'))).toBeVisible();
    await confirm.confirm();
    await expect(page.locator('[data-testid^="row-"]').filter({ hasText: pending })).toHaveCount(0, {
      timeout: 45_000,
    });
  });

  test('a dispatcher captures proof of delivery for a stop', async ({
    shell,
    page,
    t,
    api,
    cleanup,
  }) => {
    const code = `e2e-trip-${unique()}`;
    const receiver = `e2e-receiver-${unique()}`;
    await shell.open('tripManager');
    releaseAfterwards(api, cleanup, code);
    await planTrip(page, t, code);

    await page.getByRole('tab', { name: t('trips.workspace.stops') }).click();
    const stopRow = page.locator('[data-testid^="row-"]').last();
    await expect(stopRow).toBeVisible({ timeout: 45_000 });
    await stopRow.getByRole('button', { name: t('pod.record') }).click();

    const pod = new FormDialog(page, t);
    await pod.waitOpen();
    await expect(pod.root.getByText(t('pod.deliveryHint'))).toBeVisible();
    await expect(pod.root.getByText(t('pod.documentsHint'))).toBeVisible();

    // "Received by" is the one thing a proof of delivery cannot be without.
    await pod.save();
    await expect(pod.error('receiverName')).toBeVisible();

    await pod.field('receiverName').fill(receiver);
    await pod.field('notes').fill('captured by the e2e suite');
    await pod.saveAndClose();

    // The capture becomes a card on the proof-of-delivery tab.
    await page.getByRole('tab', { name: t('trips.workspace.pod') }).click();
    await expect(page.getByText(receiver).first()).toBeVisible({ timeout: 45_000 });
  });

  test('the assignment panel names who is driving, and the history records it', async ({
    shell,
    page,
    t,
    api,
    cleanup,
  }) => {
    // A staff is assignable only to a trip whose UNIT it is actually linked to
    // — `ValidateStaffAssignment` matches the staff's default transporter (or
    // a live assignment) against the trip's, and anything else is refused with
    // TRIP_STAFF_NOT_ASSIGNABLE. So the pair is built first, and the trip is
    // planned on that unit by name rather than on whichever came first.
    // An EXISTING unit, not a seeded one: the trip screen's unit picker is fed
    // by `transporterLookupByUser`, which is group-scoped, so a unit created
    // without group membership would never be offered.
    const units = await api.visibleTransporters();
    test.skip(units.length === 0, 'This account has no unit visible to the signed-in user.');
    const unit = units[0];

    const staffName = uniqueName('trip-staff');
    const staffId = await api.createStaffMember(staffName, unit.transporterId);
    cleanup.add(`staff ${staffName}`, () => api.deactivateStaffMember(staffId));

    const code = `e2e-trip-${unique()}`;
    await shell.open('tripManager');
    releaseAfterwards(api, cleanup, code);
    await planTrip(page, t, code, unit.name);

    await page.getByRole('tab', { name: t('trips.workspace.assignment') }).click();
    const panel = page.getByTestId('panel-assignment');
    await expect(panel.getByText(t('trips.assignment.none'))).toBeVisible({ timeout: 45_000 });

    await pickOption(page, panel.locator('#assignStaffId'), staffName, true);
    await panel.getByRole('button', { name: t('trips.assignment.assign') }).click();

    // Asserted INSIDE the panel: the picker keeps showing the chosen name
    // whether or not the assignment was accepted, so a page-wide match would
    // pass on a refusal.
    await expect(panel.getByText(t('trips.assignment.notAcknowledged'))).toBeVisible({
      timeout: 45_000,
    });
    await expect(panel.getByText(staffName).first()).toBeVisible();

    // Assigning a staff appends a real timeline row (`AssignTrip` calls
    // `tripEventWriter.AppendAsync`), so the history records who is driving and
    // that it came from the portal.
    await page.getByRole('tab', { name: t('trips.workspace.timeline') }).click();
    await expect(page.getByText(t('trips.timeline.events.TripAssigned')).first()).toBeVisible({
      timeout: 45_000,
    });
  });

  test('a planned trip is still fully editable, and the change lands in its history', async ({
    shell,
    page,
    t,
    api,
    cleanup,
  }) => {
    // While a trip is only planned, everything is editable — including its unit
    // and its origin. Both freeze once it is running, because re-pointing a trip
    // mid-flight would change the meaning of measurements already taken.

    const code = `e2e-trip-${unique()}`;
    await shell.open('tripManager');
    releaseAfterwards(api, cleanup, code);
    await planTrip(page, t, code);

    await page.getByRole('button', { name: t('generic.edit') }).first().click();
    const form = new FormDialog(page, t);
    await form.waitOpen();
    await expect(form.field('code')).toHaveValue(code);
    await expect(form.field('transporterId')).toBeVisible();
    await expect(form.field('originGeofencePick')).toBeVisible();

    await form.field('customerName').fill('E2E customer, renamed');
    await form.saveAndClose();

    await page.getByRole('tab', { name: t('trips.workspace.timeline') }).click();
    await expect(page.getByText(t('trips.timeline.events.TripUpdated')).first()).toBeVisible({
      timeout: 45_000,
    });
  });

  test('a round trip appends the return leg, and a planned route can be reused', async ({
    shell,
    page,
    t,
    api,
    cleanup,
  }) => {
    const code = `e2e-trip-${unique()}`;
    await shell.open('tripManager');
    releaseAfterwards(api, cleanup, code);

    await page.getByRole('button', { name: t('trips.newTrip') }).click();
    const form = new FormDialog(page, t);
    await form.waitOpen();

    await form.field('code').fill(code);
    await form.selectFirst('transporterId');
    await form
      .field('plannedStartAt')
      .fill(new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString().slice(0, 16));
    await form.select('tripType', t('trips.type.round'));

    // The origin is a PLACE, from a geofence or a point of interest. There is no
    // coordinate box, so the automatic start is judged against a real shape
    // rather than a circle around a pin.
    await expect(form.root.getByText(t('trips.origin.none'))).toBeVisible();
    await form.selectFirst('originGeofencePick');
    await expect(form.root.getByText(/^Origin set at /)).toBeVisible();
    await form.selectFirst('destinationGeofencePick');
    await form.saveAndClose();

    await expect(page.getByRole('heading', { name: code })).toBeVisible({ timeout: 60_000 });
    // The destination plus the return leg the round trip appended.
    await expect(plannerStop(page, 2)).toBeVisible({ timeout: 45_000 });

    // The route just planned is offered as the starting point for the next trip.
    const reuse = `e2e-trip-${unique()}`;
    releaseAfterwards(api, cleanup, reuse);
    await page.getByRole('button', { name: t('trips.newTrip') }).click();
    const next = new FormDialog(page, t);
    await next.waitOpen();
    await next.field('code').fill(reuse);
    await next.selectFirst('transporterId');
    await next
      .field('plannedStartAt')
      .fill(new Date(Date.now() + 26 * 60 * 60 * 1000).toISOString().slice(0, 16));
    await next.select('copyFrom', new RegExp(code));

    // Reuse fills the places in, so the trip saves without picking them again.
    await expect(next.root.getByText(/^Origin set at /)).toBeVisible({ timeout: 30_000 });
    await next.saveAndClose();
    await expect(page.getByRole('heading', { name: reuse })).toBeVisible({ timeout: 60_000 });
  });

  test('the bulk upload reports the rows it rejected', async ({ shell, page, t }) => {
    await shell.open('tripManager');
    await page.getByRole('button', { name: t('trips.import.action') }).click();

    const dialog = new FormDialog(page, t);
    await dialog.waitOpen();
    // One header, one row that names places this account does not have.
    await dialog.root
      .getByRole('textbox')
      .last()
      .fill('externalReference,code,transporter,origin,destination,plannedStartAt\nE2E,BROKEN,,,,');
    await dialog.save();

    await expect(
      dialog.root
        .getByText(t('trips.import.errors'))
        .or(dialog.root.getByText(t('trips.import.rowsRead')))
        .first()
    ).toBeVisible({ timeout: 60_000 });
    await dialog.cancel();
  });

  test('the exception filter narrows the board', async ({ shell, page, t }) => {
    await shell.open('tripManager');

    const exceptions = page.getByRole('combobox', { name: t('trips.exceptions.label') });
    await exceptions.click();
    await page.getByRole('option', { name: t('trips.exceptions.offCorridor') }).click();
    await expect(exceptions).toContainText(t('trips.exceptions.offCorridor'));

    // Either matching trips or the explicit empty state — both are answers.
    await expect(
      page.locator('[data-testid^="row-"]').first().or(page.getByText(t('trips.noTrips'))).first()
    ).toBeVisible({ timeout: 45_000 });

    await exceptions.click();
    await page.getByRole('option', { name: t('trips.exceptions.all') }).click();
  });

  test('the exception verbs of a running trip', async ({ shell, page, t, api }) => {
    const tripId = optional('E2E_TRIP_IN_TRANSIT_ID');
    test.skip(
      !tripId,
      'Set E2E_TRIP_IN_TRANSIT_ID to a trip that is actually in transit — the lifecycle is GPS-driven and cannot be simulated from a browser.'
    );

    // The board is SERVER-paged and opens unnarrowed, so one named trip is reachable
    // only through the code search — the id the variable carries is not what it matches on.
    const { tripDetail } = await api.gql<{ tripDetail: { trip: { code: string } } }>(
      'tripManagement',
      'query($tripId: UUID!) { tripDetail(query: { tripId: $tripId }) { trip { code } } }',
      { tripId }
    );

    await shell.open('tripManager');
    await page.getByPlaceholder(t('navbar.searchText')).fill(tripDetail.trip.code);

    const row = page.locator(`[data-testid="row-${tripId}"]`);
    await expect(row).toBeVisible({ timeout: 45_000 });
    await row.click();
    await page.getByRole('button', { name: t('trips.override.action') }).click();

    await expect(page.getByRole('button', { name: t('trips.actions.pause') })).toBeVisible();
    await page.getByRole('button', { name: t('trips.actions.pause') }).click();
    await expect(page.getByText(t('trips.statuses.Paused'), { exact: true }).first()).toBeVisible({
      timeout: 45_000,
    });

    await page.getByRole('button', { name: t('trips.actions.resume') }).click();
    await expect(
      page.getByText(t('trips.statuses.InProgress'), { exact: true }).first()
    ).toBeVisible({ timeout: 45_000 });
  });
});
