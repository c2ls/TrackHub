/**
 * The dashboard: the live-map tab (Units) and the history/replay tab (Positions).
 *
 * Positions come from the Router sync workers and real GPS providers, so the map
 * may legitimately be empty. What is asserted unconditionally is the structure —
 * tabs, stat cards, filter bar, map, side table, refresh counter. Marker- and
 * playback-level assertions run only when the environment says the stack has
 * live data (`E2E_HAS_POSITIONS=1`).
 */

import { test, expect, flag, optional } from '../fixtures';
import {
  accountSettingsLoaded,
  focusedUnit,
  positionsLoaded,
  settledText,
} from '../pages/dashboard';

test.describe('dashboard', () => {
  test('the units tab shows the fleet summary, filter bar, map and side list', async ({
    shell,
    page,
    t,
  }) => {
    await shell.open('dashboard');

    await expect(page.getByRole('tab', { name: t('dashboard.transportersTitle') })).toBeVisible();
    await expect(page.getByRole('tab', { name: t('dashboard.positionsTitle') })).toBeVisible();

    for (const title of [
      t('dashboard.totalTitle'),
      t('dashboard.activeTitle'),
      t('dashboard.movementTitle'),
      t('dashboard.criticalAlerts'),
    ]) {
      await expect(page.getByText(title, { exact: true })).toBeVisible();
    }

    // The geofencing tile is the one feature-gated widget on this screen; the
    // account under test has the feature on.
    await expect(page.getByText(t('dashboard.inGeofence'), { exact: true })).toBeVisible();

    for (const name of [
      t('dashboard.filterGroup'),
      t('dashboard.filterType'),
      t('dashboard.filterOperator'),
      t('dashboard.filterStatus'),
    ]) {
      await expect(page.getByRole('combobox', { name })).toBeVisible();
    }

    // Leaflet renders the OSM tiles into the map container.
    await expect(page.getByRole('link', { name: 'Leaflet' })).toBeVisible({ timeout: 60_000 });
    await expect(page.getByRole('button', { name: 'Zoom in' })).toBeVisible();
    await expect(page.getByRole('columnheader').first()).toBeVisible();
  });

  test('the status filter narrows the map without changing the totals', async ({
    shell,
    page,
    t,
  }) => {
    const loaded = positionsLoaded(page);
    await shell.open('dashboard');
    await loaded;

    const total = page
      .getByText(t('dashboard.totalTitle'), { exact: true })
      .locator('xpath=..')
      .getByRole('heading');
    await expect(total).toBeVisible({ timeout: 60_000 });
    const before = await settledText(total);

    const status = page.getByRole('combobox', { name: t('dashboard.filterStatus') });
    await status.click();
    await page.getByRole('option', { name: t('dashboard.statusOffline') }).click();
    await expect(status).toContainText(t('dashboard.statusOffline'));

    // Filters narrow the MAP only — the stat cards keep reporting the full set.
    await expect(total).toHaveText(before);

    await status.click();
    await page.getByRole('option', { name: t('dashboard.allStatuses') }).click();
  });

  test('the points-of-interest, follow and trail chips toggle', async ({ shell, page, t }) => {
    await shell.open('dashboard');

    const poi = page.getByRole('button', { name: t('dashboard.poiLayer') });
    await expect(poi).toHaveAttribute('aria-pressed', 'false');
    await poi.click();
    await expect(poi).toHaveAttribute('aria-pressed', 'true');
    await poi.click();
    await expect(poi).toHaveAttribute('aria-pressed', 'false');

    // Follow needs a selected unit; without one it stays disabled.
    await expect(page.getByRole('button', { name: t('dashboard.followMode') })).toBeDisabled();

    const trail = page.getByRole('button', { name: t('dashboard.trail') });
    await trail.click();
    await expect(trail).toHaveAttribute('aria-pressed', 'true');
    await trail.click();
    await expect(trail).toHaveAttribute('aria-pressed', 'false');
  });

  test('the side table names its columns, searches, and a row click focuses the map', async ({
    shell,
    page,
    t,
  }) => {
    await shell.open('dashboard');

    // The four columns a dispatcher reads the fleet by.
    for (const column of ['status', 'name', 'dateTime', 'speedShort'] as const) {
      await expect(
        page.getByRole('columnheader', { name: t(`transporterMap.${column}`) })
      ).toBeVisible({ timeout: 60_000 });
    }

    const rows = page.locator('[data-testid^="row-"]');
    const total = await rows.count();
    if (total === 0) {
      // An account with no reporting unit has nothing to focus; the empty table
      // is itself the correct answer.
      await expect(focusedUnit(page)).toBeVisible();
      return;
    }

    const unit = (await rows.first().innerText()).split('\n').find((line) => line.trim() !== '');
    expect(unit).toBeDefined();

    // Selecting a unit hands it to the map, which is what pans and opens its
    // marker — the list and the map are one selection, not two.
    await rows.first().click();
    await expect(focusedUnit(page)).not.toHaveAttribute(
      'data-selected-marker',
      '',
      { timeout: 30_000 }
    );

    // With a unit selected, following it is finally offered.
    await expect(page.getByRole('button', { name: t('dashboard.followMode') })).toBeEnabled();

    // The navbar search filters the same list.
    await page.getByPlaceholder(t('navbar.searchText')).fill('e2e-no-such-unit-anywhere');
    await expect(rows).toHaveCount(0, { timeout: 30_000 });
    await page.getByPlaceholder(t('navbar.searchText')).fill('');
    await expect(rows.first()).toBeVisible({ timeout: 30_000 });
  });

  test('the refresh counter counts down on the live map', async ({ shell, page }) => {
    const settings = accountSettingsLoaded(page);
    await shell.open('dashboard');
    await settings;

    // Rendered only when the account enables automatic refresh — a decision that is only
    // knowable once the settings answer, so the skip must not be judged before then.
    const counter = page.locator('.mapcontrol');
    await counter.first().waitFor({ state: 'attached', timeout: 10_000 }).catch(() => null);
    test.skip(
      (await counter.count()) === 0,
      'The account has automatic map refresh switched off, so no counter is rendered.'
    );

    const first = Number((await counter.innerText()).replace(/\D/g, ''));
    await expect
      .poll(async () => Number((await counter.innerText()).replace(/\D/g, '')), { timeout: 20_000 })
      .toBeLessThan(first);
  });

  test('the positions tab asks for a unit and a date range before searching', async ({
    shell,
    page,
    t,
  }) => {
    await shell.open('dashboard');
    await page.getByRole('tab', { name: t('dashboard.positionsTitle') }).click();

    await expect(page.getByLabel(t('filters.startDate'))).toBeVisible();
    await expect(page.getByLabel(t('filters.endDate'))).toBeVisible();
    const search = page.getByRole('button', { name: t('filters.search') });

    await search.click();
    // The unit picker preselects the first unit, so the missing halves are the
    // dates — and the form says so instead of asking the backend.
    await expect(page.locator('#startDate-helper-text')).toHaveText(
      t('validation.required', { field: 'startDate' })
    );
    await expect(page.locator('#endDate-helper-text')).toHaveText(
      t('validation.required', { field: 'endDate' })
    );

    // The CSV export stays disabled until a search returns something.
    await expect(page.getByRole('button', { name: t('replay.export') })).toBeDisabled();
  });

  test('the positions tab replays a unit history when the stack has one', async ({
    shell,
    page,
    t,
  }) => {
    test.skip(
      !flag('E2E_HAS_POSITIONS'),
      'Set E2E_HAS_POSITIONS=1 when the stack carries live GPS history — replay cannot be asserted without recorded points.'
    );

    await shell.open('dashboard');
    await page.getByRole('tab', { name: t('dashboard.positionsTitle') }).click();

    // Replay TrackHub's OWN stored history. The source defaults to the GPS provider,
    // which only answers where live provider credentials are configured; the recorded
    // points E2E_HAS_POSITIONS promises are the ones in TrackHub.
    const stored = page.getByRole('button', { name: t('replay.sourceTrackHub') });
    if ((await stored.count()) > 0) {
      await stored.click();
      await expect(stored).toHaveAttribute('aria-pressed', 'true');
    }

    const until = new Date(optional('E2E_POSITIONS_UNTIL') ?? Date.now());
    const from = new Date(until.getTime() - 7 * 24 * 60 * 60 * 1000).toISOString().slice(0, 16);
    const to = until.toISOString().slice(0, 16);
    await page.locator('#startDate').fill(from);
    await page.locator('#endDate').fill(to);
    await page.locator('#selectedItem').click();
    // Take the first unit the deployment offers rather than naming one.
    await page.getByRole('option').first().click();
    await page.getByRole('button', { name: t('filters.search') }).click();

    await expect(page.getByRole('button', { name: t('replay.export') })).toBeEnabled({
      timeout: 60_000,
    });
  });
});
