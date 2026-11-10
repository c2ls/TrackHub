/**
 * The shell every signed-in screen renders inside: sidenav, navbar (contextual
 * help, notification bell, mini toggle, sign off), the configurator drawer, the
 * announcement banner and the routing fallbacks.
 */

import fs from 'node:fs';
import path from 'node:path';
import { test, expect, flag, unique } from '../fixtures';
import { NAV_KEYS, NAV_ROUTES } from '../pages/shell';
import type { NavKey } from '../pages/shell';
import { PORTAL_ROOT } from '../fixtures/env';

interface HelpManifest {
  languages: string[];
  topics: { id: string; screens?: string[]; i18n: Record<string, { title: string }> }[];
}

const manifest = (): HelpManifest =>
  JSON.parse(
    fs.readFileSync(path.join(PORTAL_ROOT, 'public', 'help', 'manifest.json'), 'utf8')
  ) as HelpManifest;

/** Resolves once an account-settings save has actually answered. */
const settingsSaved = (page: import('@playwright/test').Page) =>
  page
    .waitForResponse(
      (response) => (response.request().postData() ?? '').includes('UpdateAccountSettings'),
      { timeout: 60_000 }
    )
    .catch(() => null);

test.describe('shell', () => {
  test('the administrator sidenav offers every route, and each one navigates', async ({
    shell,
    page,
  }) => {
    await shell.enter();

    // The seeded administrator holds Administrator + Manager and the account has
    // every feature on, so the full route table is reachable.
    await expect.poll(() => shell.visibleNavKeys(), { timeout: 30_000 }).toEqual([...NAV_KEYS]);

    for (const key of NAV_KEYS) {
      if (key === 'platformStatus') continue; // chrome-less: covered by its own spec
      await shell.open(key);
      expect(new URL(page.url()).pathname).toBe(NAV_ROUTES[key]);
      await expect(shell.nav(key)).toBeVisible();
    }
  });

  test('the help button opens the topic for the screen it was pressed on', async ({
    shell,
    page,
    lang,
  }) => {
    const topics = manifest().topics;
    const screensToCheck: NavKey[] = ['dashboard', 'manageAdmin', 'systemAdmin', 'reports'];

    for (const key of screensToCheck) {
      await shell.open(key);
      const dialog = await shell.openHelp();

      const expected = topics.find((topic) => topic.screens?.includes(key));
      expect(expected, `no help topic is registered for the "${key}" screen`).toBeDefined();
      const title = expected!.i18n[lang]?.title ?? expected!.i18n.en.title;
      // The dialog's own title element. Matching by name alone is ambiguous:
      // the topic body repeats the words in its own headings, and the id is
      // duplicated on the MUI title wrapper (reported as a finding).
      await expect(dialog.locator('span#help-dialog-title')).toHaveText(title);

      await page.keyboard.press('Escape');
      await expect(dialog).toBeHidden();
    }
  });

  test('the notification bell opens its feed', async ({ shell, page, t }) => {
    await shell.enter();
    await shell.bellButton.click();

    const menu = page.getByRole('menu');
    await expect(menu).toBeVisible();
    // Either the empty state or real deliveries — both are correct answers.
    const empty = menu.getByText(t('notificationBell.empty'));
    const items = menu.getByRole('menuitem');
    await expect(empty.or(items.first()).first()).toBeVisible();

    await page.keyboard.press('Escape');
    await expect(menu).toBeHidden();
  });

  test('the mini-sidenav toggle collapses and restores the sidenav', async ({ shell, page }) => {
    await shell.enter();
    const link = shell.nav('dashboard');
    const widthOf = async (): Promise<number> => (await link.boundingBox())?.width ?? 0;

    const expanded = await widthOf();
    expect(expanded).toBeGreaterThan(150);

    // The icon ligature flips between 'menu' and 'menu_open' with the state, so
    // the locator has to match both.
    const toggle = page.getByText(/^menu(_open)?$/).first();
    await toggle.click();
    // Collapsed, the sidenav keeps only the icon rail, so its items get narrow.
    await expect.poll(widthOf).toBeLessThan(expanded / 2);

    await toggle.click();
    await expect.poll(widthOf).toBeGreaterThan(150);
  });

  test('the configurator opens with the account map settings', async ({ shell, page, t }) => {
    await shell.enter();
    await shell.configuratorButton.click();

    await expect(shell.configuratorTitle).toBeInViewport();
    await expect(page.getByRole('combobox', { name: t('settings.maps') })).toBeVisible();
    await expect(
      page.getByRole('spinbutton', { name: t('settings.onlineInterval') })
    ).toBeVisible();
    await expect(
      page.getByRole('spinbutton', { name: t('settings.refreshMapInterval') })
    ).toBeVisible();
  });

  test('the configurator saves a map refresh interval that survives a reload', async ({
    shell,
    page,
    t,
  }) => {
    // Saving confirms through the app's own toast, not a native dialog.
    const saveToast = page.getByRole('alert').filter({ hasText: t('settings.saveMessage') });

    await shell.enter();
    await shell.configuratorButton.click();
    await expect(shell.configuratorTitle).toBeInViewport();

    const interval = page.getByRole('spinbutton', { name: t('settings.refreshMapInterval') });
    const original = await interval.inputValue();
    const changed = original === '90' ? '120' : '90';

    await interval.fill(changed);
    const saved = settingsSaved(page);
    await page.getByRole('button', { name: t('generic.save') }).last().click();
    await expect(saveToast).toBeVisible();
    await saved;

    await shell.reloadTo('dashboard');
    await shell.configuratorButton.click();
    await expect(interval).toHaveValue(changed);

    // Leave the account setting as it was found.
    await interval.fill(original);
    const restored = settingsSaved(page);
    await page.getByRole('button', { name: t('generic.save') }).last().click();
    await expect(saveToast).toBeVisible();
    await restored;
  });

  test('the configurator refuses a refresh interval below the platform minimum', async ({
    shell,
    page,
    t,
  }) => {
    // A refused save must not confirm: the success toast never appears.
    const saveToast = page.getByRole('alert').filter({ hasText: t('settings.saveMessage') });

    await shell.enter();
    await shell.configuratorButton.click();
    await expect(shell.configuratorTitle).toBeInViewport();

    const interval = page.getByRole('spinbutton', { name: t('settings.refreshMapInterval') });
    const original = await interval.inputValue();
    await interval.fill('45');
    await page.getByRole('button', { name: t('generic.save') }).last().click();

    await expect(
      page.getByText(t('settings.validation.refreshMapInterval', { min: 60 }))
    ).toBeVisible();
    await expect(saveToast).toBeHidden();

    await interval.fill(original);
  });

  test('every screen names itself in the top bar', async ({ shell }) => {
    await shell.enter();

    for (const key of NAV_KEYS) {
      // The status page is public and carries no shell.
      if (key === 'platformStatus') continue;
      await shell.open(key);

      const label = await shell.nav(key).locator('.MuiListItemText-root').innerText();
      await expect(shell.screenTitle).toHaveText(label, {
        timeout: 30_000,
      });
    }
  });

  test('the configurator changes the map provider and the choice survives a reload', async ({
    shell,
    page,
    t,
  }) => {
    // Switching the account to Google renders every map screen against the Google Maps
    // API: without a key authorized for the portal's origin the map is broken until the
    // setting is put back, so the flip is only driven where the environment can serve it.
    test.skip(
      !flag('E2E_GOOGLE_MAPS'),
      'Set E2E_GOOGLE_MAPS=1 when the stack has a Maps key authorized for the portal origin.'
    );

    // Saving confirms through the app's own toast, not a native dialog.
    const saveToast = page.getByRole('alert').filter({ hasText: t('settings.saveMessage') });

    await shell.enter();
    await shell.configuratorButton.click();
    await expect(shell.configuratorTitle).toBeInViewport();

    const provider = page.getByRole('combobox', { name: t('settings.maps') });
    const original = (await provider.innerText()).trim();
    const target = original === 'OSM' ? 'Google' : 'OSM';

    await provider.click();
    await page.getByRole('option', { name: target, exact: true }).click();
    const saved = settingsSaved(page);
    await page.getByRole('button', { name: t('generic.save') }).last().click();
    await expect(saveToast).toBeVisible();
    await saved;

    await shell.reloadTo('dashboard');
    await shell.configuratorButton.click();
    await expect(page.getByRole('combobox', { name: t('settings.maps') })).toHaveText(target, {
      timeout: 30_000,
    });

    // Put the account setting back the way it was found: the provider decides
    // which map every other test renders against.
    await page.getByRole('combobox', { name: t('settings.maps') }).click();
    await page.getByRole('option', { name: original, exact: true }).click();
    const restored = settingsSaved(page);
    await page.getByRole('button', { name: t('generic.save') }).last().click();
    await expect(saveToast).toBeVisible();
    await restored;
    await shell.reloadTo('dashboard');
    await shell.configuratorButton.click();
    await expect(page.getByRole('combobox', { name: t('settings.maps') })).toHaveText(original, {
      timeout: 30_000,
    });
  });

  test('an active announcement reaches every signed-in screen', async ({
    shell,
    page,
    t,
    cleanup,
  }) => {
    // The banner waits out a 60 s server-side output cache (see below), which
    // does not fit the suite's default per-test budget.
    test.setTimeout(240_000);

    const messageEn = `E2E announcement ${unique()}`;

    // /status never triggers the sign-in flow (it is the page you check when you
    // CANNOT sign in), so the administrator tier only appears when the session
    // already exists — a client-side navigation from the shell, not a fresh load.
    await shell.open('platformStatus');
    await expect(page.getByRole('heading', { name: t('platformStatus.title') })).toBeVisible();
    const manage = page.getByRole('button', { name: t('platformStatus.manage.open') });
    await expect(manage).toBeVisible({ timeout: 60_000 });
    await manage.click();

    const dialog = page.getByTestId('dialog-form');
    await expect(dialog).toBeVisible();
    await dialog.getByRole('textbox', { name: t('platformStatus.manage.messageEn') }).fill(messageEn);
    await dialog.getByRole('textbox', { name: t('platformStatus.manage.messageEs') }).fill(messageEn);
    await dialog.getByRole('button', { name: t('generic.save') }).click();
    // A successful save resets the draft, so the message moves out of the inputs
    // and into the 'existing announcements' list below them.
    await expect(
      dialog.getByRole('textbox', { name: t('platformStatus.manage.messageEn') })
    ).toHaveValue('', { timeout: 30_000 });
    await expect(dialog.getByText(messageEn, { exact: true })).toBeVisible({ timeout: 30_000 });

    cleanup.add(`announcement ${messageEn}`, async () => {
      await shell.open('platformStatus');
      const reopen = page.getByRole('button', { name: t('platformStatus.manage.open') });
      await expect(reopen).toBeVisible({ timeout: 60_000 });
      await reopen.click();
      // The manager lists each announcement as its text plus a pair of icon
      // buttons two levels up; the text node itself holds no button.
      const listed = page.getByTestId('dialog-form').getByText(messageEn, { exact: true });
      if ((await listed.count()) > 0) {
        await listed
          .locator('xpath=ancestor::div[2]')
          .getByRole('button', { name: t('generic.delete') })
          .click();
        await expect(listed).toBeHidden({ timeout: 30_000 });
      }
    });

    await page.keyboard.press('Escape');

    // The anonymous announcements endpoint is output-cached for 60 s
    // (`Manager/Web/Program.cs`), so a brand-new announcement reaches the banner
    // only once that window rolls over. Reload until it does rather than
    // asserting on a single fetch, which passes or fails on where in the window
    // the test happened to land.
    const banner = shell.announcement.filter({ hasText: messageEn });
    await expect
      .poll(
        async () => {
          if (await banner.isVisible().catch(() => false)) return true;
          await shell.reloadTo('dashboard');
          return banner.isVisible().catch(() => false);
        },
        { timeout: 150_000, intervals: [5_000] }
      )
      .toBe(true);
  });

  test('an unknown route redirects to the dashboard', async ({ shell, page }) => {
    await shell.enter();
    // A client-side navigation: a document load would re-run the sign-in bounce
    // and land on the dashboard for a different reason entirely.
    await page.evaluate(() => window.history.pushState({}, '', '/no-such-screen'));
    await page.getByTestId('nav-profile').click();
    await page.getByTestId('nav-dashboard').click();
    await expect(page).toHaveURL(/\/dashboard$/);

    await page.goto('/no-such-screen');
    await expect(page.getByTestId('nav-dashboard')).toBeVisible({ timeout: 60_000 });
    await expect(page).toHaveURL(/\/dashboard$/);
  });

  test('the auth error page renders its message and its way back', async ({ anonPage, t }) => {
    // The page reads the reason the callback stored; without one it bounces to
    // the dashboard, so the state is seeded the way the callback would.
    await anonPage.goto('/status');
    await anonPage.evaluate(() => sessionStorage.setItem('auth_error', 'token_exchange_failed'));
    await anonPage.goto('/error');

    await expect(anonPage.getByRole('heading', { name: t('authError.title') })).toBeVisible();
    await expect(anonPage.getByRole('alert')).toContainText(t('authError.tokenExchangeFailed'));
    await expect(anonPage.getByRole('button', { name: t('authError.goBack') })).toBeVisible();
    await expect(anonPage.getByRole('link', { name: t('platformStatus.viewStatus') })).toBeVisible();
  });
});
