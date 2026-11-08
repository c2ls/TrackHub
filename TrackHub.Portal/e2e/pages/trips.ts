/**
 * The Trips screen: the dispatch board, the new-trip dialog and the planner's
 * stop list.
 *
 * The lifecycle is automatic (spec 11a) — a trip starts when its unit reaches
 * the origin zone and completes at its last stop — so what a browser can drive
 * is planning and the exception verbs, not the transitions themselves.
 */

import { expect } from '@playwright/test';
import type { Locator, Page } from '@playwright/test';
import { FormDialog } from './dialogs';
import type { Translator } from '../fixtures/i18n';
import type { ApiClient } from '../fixtures/api';
import type { CleanupRegistry } from '../fixtures/data';

/** A row of the dispatch board, found by the trip code a human reads. */
export const boardRow = (page: Page, code: string): Locator =>
  page.locator('[data-testid^="row-"]').filter({ hasText: code }).first();

/** A row of the planner's stop list, addressed by its visit order. */
export const plannerStop = (page: Page, sequence: number): Locator =>
  page.getByTestId(`stop-${sequence}`);

/** Every planner stop row, for finding one by the name it was given. */
export const plannerStopNamed = (page: Page, name: string): Locator =>
  page.getByTestId(/^stop-/).filter({ hasText: name });

/**
 * Plans a trip the way a dispatcher does: a code, a unit, a planned start, an
 * origin picked from a geofence and one destination picked from a geofence.
 *
 * Returns with the workspace already open on the new trip — `saveTrip` selects
 * what it created, so clicking the board row would toggle it shut again.
 */
export async function planTrip(
  page: Page,
  t: Translator,
  code: string,
  transporterName?: string
): Promise<void> {
  await page.getByRole('button', { name: t('trips.newTrip') }).click();
  const form = new FormDialog(page, t);
  await form.waitOpen();

  await form.field('code').fill(code);
  await form.field('customerName').fill('E2E customer');
  // A named unit matters when the trip has to carry a staff: a staff is only
  // assignable to a trip whose unit it is actually linked to.
  if (transporterName) {
    // The unit picker searches the server, so the name is typed rather than scrolled to.
    await form.field('transporterId').fill(transporterName);
    await page.getByRole('option', { name: transporterName, exact: true }).click();
  } else {
    await form.selectFirst('transporterId');
  }
  await form
    .field('plannedStartAt')
    .fill(new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString().slice(0, 16));

  // Origin and destinations are PLACES: a geofence or a point of interest.
  await form.selectFirst('originGeofencePick');
  await form.selectFirst('destinationGeofencePick');

  await form.saveAndClose();
  await expect(page.getByRole('heading', { name: code })).toBeVisible({ timeout: 60_000 });
}

/**
 * Registers the release of a trip created through the UI.
 *
 * A `Created` trip holds its unit's arming slot, so a leftover silently absorbs
 * the automatic start of every trip planned after it (spec 11b). `releaseTrip`
 * walks delete → abort → cancel, because a trip that has picked up any history
 * can no longer be deleted.
 */
export function releaseAfterwards(
  api: ApiClient,
  cleanup: CleanupRegistry,
  code: string
): void {
  cleanup.add(`trip ${code}`, async () => {
    const found = await api.tryGql<{ trips: { items: { tripId: string; code: string }[] } }>(
      'tripManagement',
      'query($search: String) { trips(query: { search: $search, take: 50 }) { items { tripId code } } }',
      { search: code }
    );
    const trip = found?.trips.items.find((row) => row.code === code);
    if (trip) await api.releaseTrip(trip.tripId);
  });
}
