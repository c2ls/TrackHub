/**
 * The shared section control (`controls/Accordions/TableAccordion` + `Table` +
 * `ServerSearch` + `ServerPagination`).
 *
 * Nearly every administrative screen is a stack of these, so one page object
 * covers System Admin, Account Management and GPS Integration alike. Sections
 * are addressed by the language-independent `sectionKey` the control renders as
 * `data-testid="section-<key>"`.
 */

import { expect } from '@playwright/test';
import type { Locator, Page } from '@playwright/test';
import type { Translator } from '../fixtures/i18n';

export class Section {
  constructor(
    readonly page: Page,
    readonly key: string,
    private readonly t: Translator
  ) {}

  get root(): Locator {
    return this.page.getByTestId(`section-${this.key}`);
  }

  get header(): Locator {
    return this.page.getByTestId(`section-${this.key}-header`);
  }

  /** The section's own "Add" affordance — absent without the write permission. */
  get addButton(): Locator {
    return this.page.getByTestId(`section-${this.key}-add`);
  }

  get searchBox(): Locator {
    return this.root.getByRole('textbox', { name: this.t('filters.search') });
  }

  /** Every rendered row of the section's table. */
  get rows(): Locator {
    return this.root.locator('[data-testid^="row-"]');
  }

  row(text: string | RegExp): Locator {
    return this.rows.filter({ hasText: text }).first();
  }

  rowById(id: string): Locator {
    return this.root.getByTestId(`row-${id}`);
  }

  /** "Showing x–y of z" — the server-paged footer. */
  get rangeLabel(): Locator {
    return this.root.getByText(/\d+\s*[–-]\s*\d+/).first();
  }

  get nextPageButton(): Locator {
    return this.root.getByTestId('pagination-next');
  }

  get previousPageButton(): Locator {
    return this.root.getByTestId('pagination-previous');
  }

  async isExpanded(): Promise<boolean> {
    // `aria-expanded` on the summary button is the accessible contract, and the
    // one MUI keeps in sync with the panel — no class names.
    return (await this.header.getAttribute('aria-expanded')) === 'true';
  }

  /**
   * Expands the section and waits for the accordion to actually open.
   *
   * The click is retried once: expanding a section above this one reflows the
   * page, and a header that moves out from under the pointer mid-click swallows
   * the toggle.
   */
  async expand(): Promise<this> {
    await expect(this.root).toBeVisible({ timeout: 60_000 });
    for (let attempt = 0; attempt < 3; attempt += 1) {
      if (await this.isExpanded()) return this;
      await this.header.click();
      try {
        await expect(this.header).toHaveAttribute('aria-expanded', 'true', { timeout: 5_000 });
        return this;
      } catch {
        // Fall through and try again.
      }
    }
    await expect(this.header).toHaveAttribute('aria-expanded', 'true');
    return this;
  }

  /** Collapses the section, retried for the same reflow reason as `expand`. */
  async collapse(): Promise<void> {
    for (let attempt = 0; attempt < 3; attempt += 1) {
      if (!(await this.isExpanded())) return;
      await this.header.click();
      try {
        await expect(this.header).toHaveAttribute('aria-expanded', 'false', { timeout: 5_000 });
        return;
      } catch {
        // Fall through and try again.
      }
    }
    await expect(this.header).toHaveAttribute('aria-expanded', 'false');
  }

  /**
   * Types into the section's server-side search box. The draft is debounced
   * (350 ms) before it reaches the query, so callers assert on the ROW, never on
   * a timer. The query's response is awaited too: a row already on screen from
   * the unfiltered page passes the visibility check before the debounced fetch
   * raises the loading overlay, and the next click would land on the overlay.
   */
  async search(text: string): Promise<void> {
    if ((await this.searchBox.inputValue()) === text) return;
    const applied = this.page
      .waitForResponse((response) => (response.request().postData() ?? '').includes(JSON.stringify(text)), {
        timeout: 30_000,
      })
      .catch(() => null);
    await this.searchBox.fill(text);
    await applied;
  }

  /** Finds one row by a search term, proving the server-side search works. */
  async findRow(text: string): Promise<Locator> {
    if (await this.searchBox.isVisible().catch(() => false)) {
      await this.search(text);
      const row = this.row(text);
      await expect(row).toBeVisible({ timeout: 30_000 });
      return row;
    }
    return this.findRowAnyPage(text);
  }

  /**
   * Finds a row in a CLIENT-paged section, walking the pages if needed.
   *
   * Sections without a server-side search render the shared `Table`, which pages
   * ten rows at a time in the browser — a freshly created row is frequently not
   * on the first page, and asserting only there would fail for the wrong reason.
   * The pager is MUI's own; its button labels are English regardless of the
   * portal's language because the app installs no MUI locale.
   */
  async findRowAnyPage(
    text: string | RegExp,
    timeout = 45_000,
    within?: Locator
  ): Promise<Locator> {
    // A section can hold SEVERAL independently paged tables — the toll catalog
    // holds three — and each carries its own pager. Without `within`, the first
    // one is walked; pass the table's own scope to walk one of the others.
    const scope = within ?? this.root;
    const rowIn = (): Locator => scope.locator('[data-testid^="row-"]').filter({ hasText: text }).first();
    const next = scope.getByRole('button', { name: 'Go to next page' }).first();
    const previous = scope.getByRole('button', { name: 'Go to previous page' }).first();
    const deadline = Date.now() + timeout;

    // The sweep is REPEATED, not single-pass: the pager only appears once more
    // than one page of rows has arrived, so a walk that starts while the query
    // is still in flight sees no pager at all and would conclude, wrongly, that
    // page one is the whole result set.
    while (Date.now() < deadline) {
      for (let page = 0; page < 50; page += 1) {
        if (await rowIn().isVisible().catch(() => false)) return rowIn();
        if ((await next.count()) === 0 || (await next.isDisabled().catch(() => true))) break;
        await next.click();
      }
      // Rewind for the next sweep.
      for (let page = 0; page < 50; page += 1) {
        if ((await previous.count()) === 0 || (await previous.isDisabled().catch(() => true))) break;
        await previous.click();
      }
      // The interval BETWEEN sweeps, not a wait for the UI to settle: the loop
      // re-reads the pager every time and exits the moment the row appears.
      await this.page.waitForTimeout(500);
    }

    const row = rowIn();
    await expect(row).toBeVisible({ timeout: 5_000 });
    return row;
  }

  /** Clicks a row-level action button (Edit, Delete, Assign, …). */
  async rowAction(rowText: string, actionLabel: string): Promise<void> {
    const row = await this.findRow(rowText);
    await row.getByRole('button', { name: actionLabel }).first().click();
  }

  async clickAdd(): Promise<void> {
    await expect(this.addButton).toBeVisible();
    await this.addButton.click();
  }
}

export const section = (page: Page, key: string, t: Translator): Section =>
  new Section(page, key, t);
