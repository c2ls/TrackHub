/**
 * The Dashboard: the live Units tab and the historical Positions tab.
 *
 * Both tabs render a map beside a table, and the selection is shared between
 * them — the map mirrors the selected unit onto its container, which is what
 * pans it and opens the marker.
 */

import { expect } from '@playwright/test';
import type { Locator, Page } from '@playwright/test';
import type { Translator } from '../fixtures/i18n';

/** A row of the fleet table beside the live map. */
export const unitRow = (page: Page, text: string): Locator =>
  page.locator('[data-testid^="row-"]').filter({ hasText: text }).first();

/** Every row of whichever table the open tab renders. */
export const rows = (page: Page): Locator => page.locator('[data-testid^="row-"]');

/** The unit the live map is currently focused on. */
export const focusedUnit = (page: Page): Locator => page.locator('[data-selected-marker]');

/** One of the three map toggles, which report their state as `aria-pressed`. */
export const toggleChip = (page: Page, t: Translator, key: 'poiLayer' | 'followMode' | 'trail') =>
  page.getByRole('button', { name: t(`dashboard.${key}`) });

export async function openTab(
  page: Page,
  t: Translator,
  key: 'transporters' | 'positions'
): Promise<void> {
  await page.getByRole('tab', { name: t(`dashboard.${key}Title`) }).click();
}

/** Resolves once the live-map positions query has answered. */
export const positionsLoaded = (page: Page, timeout = 60_000): Promise<unknown> =>
  page
    .waitForResponse(
      (response) => (response.request().postData() ?? '').includes('GetDevicePositionsByUser'),
      { timeout }
    )
    .catch(() => null);

/**
 * The text of a stat card once it has stopped moving. The cards count the live-map
 * payload and render 0 until it lands, so a value read on first paint is a placeholder,
 * not a baseline — and an account with no positions settles on 0 legitimately.
 */
export async function settledText(locator: Locator, timeout = 30_000): Promise<string> {
  let previous: string | null = null;

  await expect
    .poll(
      async () => {
        const current = (await locator.innerText()).trim();
        const settled = current === previous;
        previous = current;
        return settled;
      },
      { timeout, intervals: [250, 500, 500, 1000] }
    )
    .toBe(true);

  return previous ?? '';
}

/** Resolves once the account settings that drive the map controls have answered. */
export const accountSettingsLoaded = (page: Page, timeout = 60_000): Promise<unknown> =>
  page
    .waitForResponse(
      (response) => (response.request().postData() ?? '').includes('GetAccountSettingsByUser'),
      { timeout }
    )
    .catch(() => null);
