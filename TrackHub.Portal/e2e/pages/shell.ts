/**
 * The signed-in shell: sidenav, navbar (help, notification bell, account menu,
 * mini toggle), the configurator drawer and the announcement banner.
 */

import { expect } from '@playwright/test';
import type { Locator, Page } from '@playwright/test';
import type { Translator } from '../fixtures/i18n';
import { principalLoaded, tokenExchange } from '../fixtures/auth';

/** Every route key the sidenav can render, in the order `routes.tsx` declares them. */
export const NAV_KEYS = [
  'dashboard',
  'systemAdmin',
  'manageAdmin',
  'geofenceManager',
  'tripManager',
  'reports',
  'gpsIntegration',
  'platformStatus',
  'profile',
] as const;

export type NavKey = (typeof NAV_KEYS)[number];

/** The path each sidenav entry navigates to (`routes.tsx`). */
export const NAV_ROUTES: Record<NavKey, string> = {
  dashboard: '/dashboard',
  systemAdmin: '/systemAdmin',
  manageAdmin: '/manageAdmin',
  geofenceManager: '/geofenceManager',
  tripManager: '/tripManager',
  reports: '/reports',
  gpsIntegration: '/manage-admin/gps-integration',
  platformStatus: '/status',
  profile: '/profile',
};

export class Shell {
  /** A document load re-runs the OAuth round trip; this remembers it completed. */
  private established = false;

  constructor(
    readonly page: Page,
    private readonly t: Translator
  ) {}

  nav(key: NavKey): Locator {
    return this.page.getByTestId(`nav-${key}`);
  }

  /** Route keys the sidenav is currently offering. */
  async visibleNavKeys(): Promise<NavKey[]> {
    await expect(this.nav('dashboard')).toBeVisible({ timeout: 60_000 });
    const present: NavKey[] = [];
    for (const key of NAV_KEYS) {
      if (await this.nav(key).isVisible().catch(() => false)) present.push(key);
    }
    return present;
  }

  /** Navigates through the sidenav (a click, not a URL), then waits for the URL. */
  async navigateTo(key: NavKey): Promise<void> {
    await this.nav(key).click();
    await this.page.waitForURL(`**${NAV_ROUTES[key]}`);
  }

  /**
   * Opens a screen the way a signed-in user reaches it.
   *
   * A URL is deliberately NOT enough. The portal holds its access token in
   * memory only, so every document load re-runs the OAuth round trip against
   * the stored AuthorityServer cookie — and the callback page navigates to
   * `/dashboard` unconditionally, dropping whatever route was asked for
   * (`layouts/authentication/callback`). Typing a deep link therefore always
   * lands on the dashboard; the sidenav click that follows is a client-side
   * navigation, which keeps the token and actually arrives.
   */
  async open(key: NavKey): Promise<void> {
    await this.enter();
    if (new URL(this.page.url()).pathname !== NAV_ROUTES[key]) {
      await this.navigateTo(key);
    }
    expect(new URL(this.page.url()).pathname).toBe(NAV_ROUTES[key]);
  }

  /**
   * Establishes the session on the dashboard.
   *
   * The rendered shell is not proof of a session — `App` mounts the Sidenav
   * before the effect that redirects an unauthenticated visitor runs, so it
   * flashes on the way to the login page. The token exchange is the signal.
   */
  async enter(): Promise<void> {
    // `/status`, `/track`, `/error` and the auth interstitials render their own
    // full-page shell with no sidenav, so a session established earlier says
    // nothing about what is on screen now — go back through the dashboard.
    const path = this.page.url().startsWith('http') ? new URL(this.page.url()).pathname : null;
    const chromeless =
      path === null ||
      path === '/status' ||
      path === '/track' ||
      path === '/error' ||
      path.startsWith('/authentication/');
    if (this.established && !chromeless) {
      await this.ready();
      return;
    }
    this.established = false;
    const exchanged = tokenExchange(this.page).catch(() => null);
    const principal = principalLoaded(this.page);
    await this.page.goto('/dashboard');
    await exchanged;
    await principal;
    await this.ready();
    this.established = true;
  }

  /**
   * Reloads and comes back to the same screen — the "did the backend really keep
   * it?" check. The reload drops the in-memory token, so the return trip goes
   * through the dashboard, exactly as it does for a user who refreshes.
   */
  async reloadTo(key: NavKey): Promise<void> {
    this.established = false;
    const exchanged = tokenExchange(this.page).catch(() => null);
    const principal = principalLoaded(this.page);
    await this.page.reload();
    await exchanged;
    await principal;
    this.established = true;
    await this.open(key);
  }

  /** The shell has painted and the sidenav is present. */
  async ready(): Promise<void> {
    await expect(this.nav('dashboard')).toBeVisible({ timeout: 60_000 });
  }

  get screenTitle(): Locator {
    return this.page.getByTestId('screen-title');
  }

  get helpButton(): Locator {
    return this.page.getByRole('button', { name: this.t('help.open') });
  }

  get bellButton(): Locator {
    return this.page.getByRole('button', { name: this.t('notificationBell.title') });
  }

  get userMenuButton(): Locator {
    return this.page.getByTestId('user-menu');
  }

  get signOffButton(): Locator {
    return this.page.getByRole('menuitem', { name: this.t('navbar.signOff'), exact: true });
  }

  /** The floating settings button — rendered for managers only (`App.tsx`). */
  get configuratorButton(): Locator {
    return this.page.getByTestId('configurator-toggle');
  }

  /** The configurator drawer is permanent and slides in, so its heading's
   *  position in the viewport is what "open" means. */
  get configuratorTitle(): Locator {
    return this.page.getByRole('heading', { name: this.t('settings.title') });
  }

  get announcement(): Locator {
    return this.page.getByTestId('shell-announcement');
  }

  async openHelp(): Promise<Locator> {
    await this.helpButton.click();
    const dialog = this.page.getByTestId('dialog-help');
    await expect(dialog).toBeVisible();
    return dialog;
  }

  async signOff(): Promise<void> {
    await this.userMenuButton.click();
    await this.signOffButton.click();
  }
}
