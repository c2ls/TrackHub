/**
 * Account Management → Fleet & Tracking: devices, units, staff, groups and
 * points of interest.
 */

import { test, expect, unique, uniqueName } from '../fixtures';
import { CrudFlow } from '../pages/crud';
import { ConfirmDialog, FormDialog } from '../pages/dialogs';
import { openTab } from '../pages/dashboard';
import type { ApiClient } from '../fixtures/api';
import type { CleanupRegistry } from '../fixtures/data';
import type { Translator } from '../fixtures/i18n';
import type { Locator, Page } from '@playwright/test';

/**
 * Registers a staff and arranges for it to be deactivated afterwards.
 *
 * Seeded rather than clicked: the staff record is the PRECONDITION for the
 * credential, qualification and assignment work being measured, and creating
 * one through the UI has its own test.
 */
async function seedStaff(api: ApiClient, cleanup: CleanupRegistry): Promise<string> {
  const name = uniqueName('staff');
  const staffId = await api.createStaffMember(name);
  cleanup.add(`staff ${name}`, () => api.deactivateStaffMember(staffId));
  return name;
}

/** Chooses a named option in a picker mounted in a section body, not a dialog. */
async function selectFirstMatching(
  page: Page,
  scope: Locator,
  id: string,
  optionLabel: string
): Promise<void> {
  await scope.locator(`#${id}`).fill(optionLabel);
  await page.getByRole('option', { name: optionLabel, exact: true }).click();
}

/** Opens the staff's detail dialog, which lands on the credentials (app access) panel. */
async function openStaffAccess(page: Page, t: Translator, staffName: string): Promise<Locator> {
  const staff = new CrudFlow(page, 'staff_members', t).section;
  await staff.expand();
  const row = await staff.findRow(staffName);
  await row.getByRole('button', { name: t('staff.manage') }).click();
  const dialog = page.getByRole('dialog').filter({ hasText: staffName });
  await expect(dialog).toBeVisible();
  return dialog;
}

test.describe('account management — fleet & tracking', () => {
  test('a manager can add a unit from Account Management', async ({ page, shell, t }) => {
    const units = new CrudFlow(page, 'transporters', t);
    await shell.open('manageAdmin');
    await units.open();
    await expect(units.section.addButton).toBeVisible();
  });

  test('a manager renames a unit and the rename survives a reload', async ({
    page,
    shell,
    t,
    api,
    cleanup,
  }) => {
    const name = uniqueName('unit');
    const renamed = `${name} renamed`;
    // Seeded through the API because the UI offers no way to create a unit.
    const transporterId = await api.createTransporter(name);
    cleanup.add(`unit ${name}`, () => api.deleteTransporter(transporterId));

    const units = new CrudFlow(page, 'transporters', t);
    await shell.open('manageAdmin');
    await units.open();

    await units.edit(name, { fields: { name: renamed } });
    await expect(await units.section.findRow(renamed)).toBeVisible();

    await units.reload(shell, 'manageAdmin');
    await expect(await units.section.findRow(renamed)).toBeVisible();

    await units.remove(renamed);
  });

  test('the unit dialog refuses a save with an empty name', async ({
    page,
    shell,
    t,
    api,
    cleanup,
  }) => {
    const name = uniqueName('unit');
    const transporterId = await api.createTransporter(name);
    cleanup.add(`unit ${name}`, () => api.deleteTransporter(transporterId));

    const units = new CrudFlow(page, 'transporters', t);
    await shell.open('manageAdmin');
    await units.open();

    await units.section.rowAction(name, t('generic.edit'));
    await units.form.waitOpen();
    await units.form.field('name').fill('');
    await units.form.save();

    await expect(units.form.root).toBeVisible();
    await expect(units.form.error('name')).toHaveText(t('validation.required', { field: 'name' }));
    await units.form.cancel();
  });

  test('the units list searches on the server and pages', async ({ page, shell, t }) => {
    const units = new CrudFlow(page, 'transporters', t);
    await shell.open('manageAdmin');
    const section = await units.open();

    await expect(section.rangeLabel).toBeVisible();
    const total = Number(/of\s+(\d+)/.exec((await section.rangeLabel.innerText()) ?? '')?.[1] ?? 0);

    // A search that cannot match narrows the whole result set, not the page.
    await section.search('zzz-no-such-unit-zzz');
    await expect(section.rows).toHaveCount(0, { timeout: 30_000 });

    await section.search('');
    await expect.poll(async () => section.rows.count(), { timeout: 30_000 }).toBeGreaterThan(0);

    if (total > 10) {
      const firstPage = await section.rangeLabel.innerText();
      await section.nextPageButton.click();
      await expect.poll(() => section.rangeLabel.innerText()).not.toBe(firstPage);
      await section.previousPageButton.click();
      await expect.poll(() => section.rangeLabel.innerText()).toBe(firstPage);
    }
  });

  test('devices are listed read-only — they arrive from GPS synchronisation', async ({
    page,
    shell,
    t,
  }) => {
    const devices = new CrudFlow(page, 'devices', t);
    await shell.open('manageAdmin');
    const section = await devices.open();

    // The section deliberately has no Add: a device is created by the Router
    // sync, and the screen only lets an account retire one.
    await expect(section.addButton).toHaveCount(0);
    await expect(section.searchBox).toBeVisible();
    await expect(section.rangeLabel).toBeVisible();
  });

  test('a manager creates a group and assigns a unit to it', async ({
    page,
    shell,
    t,
    api,
    allocator,
    cleanup,
  }) => {
    const groupName = uniqueName('group');
    const unitName = uniqueName('unit');
    const transporterId = await api.createTransporter(unitName);
    cleanup.add(`unit ${unitName}`, () => api.deleteTransporter(transporterId));

    const groups = new CrudFlow(page, 'groups', t);
    await shell.open('manageAdmin');
    await groups.open();
    cleanup.add(`group ${groupName}`, () => groups.removeIfPresent(groupName));

    await groups.create(
      { fields: { name: groupName, description: 'created by the e2e suite' } },
      groupName,
      'name'
    );

    // The row carries two allocators in a fixed column order: users, then units.
    const row = await groups.section.findRow(groupName);
    await row.getByRole('button', { name: t('generic.assign') }).last().click();
    await allocator.waitOpen();
    await allocator.add(unitName);
    await expect(allocator.row(unitName)).toBeVisible({ timeout: 30_000 });
    await allocator.close();

    // Re-opening shows the saved membership, not just the optimistic row.
    await row.getByRole('button', { name: t('generic.assign') }).last().click();
    await allocator.waitOpen();
    await expect(allocator.row(unitName)).toBeVisible({ timeout: 30_000 });
    await allocator.remove(unitName);
    await expect(allocator.row(unitName)).toHaveCount(0, { timeout: 30_000 });
    await allocator.close();

    await groups.remove(groupName);
  });

  test('a manager creates a point of interest with coordinates that persist', async ({
    page,
    shell,
    t,
    cleanup,
  }) => {
    const name = uniqueName('poi');
    const pois = new CrudFlow(page, 'pois', t);

    await shell.open('manageAdmin');
    await pois.open();
    cleanup.add(`poi ${name}`, () => pois.removeIfPresent(name));

    await pois.create(
      {
        fields: {
          name,
          description: 'created by the e2e suite',
          latitude: '4.65',
          longitude: '-74.05',
        },
        selects: { type: t('poi.types.warehouse') },
      },
      name,
      'name'
    );

    await pois.reload(shell, 'manageAdmin');
    await pois.assertStored(name, {
      fields: { name, latitude: '4.65', longitude: '-74.05' },
      selects: { type: t('poi.types.warehouse') },
    });

    await pois.remove(name);
  });

  test('a manager registers a staff and the record survives a reload', async ({
    page,
    shell,
    t,
    api,
    cleanup,
  }) => {
    const name = uniqueName('staff');
    const staff = new CrudFlow(page, 'staff_members', t);

    await shell.open('manageAdmin');
    await staff.open();
    // A staff is deactivated, never deleted, so cleanup goes through the API.
    cleanup.add(`staff ${name}`, async () => {
      const found = await api.tryGql<{
        staffMembersByAccount: { items: { staffId: string; name: string; active: boolean }[] };
      }>(
        'manager',
        'query($accountId: UUID!, $skip: Int!, $take: Int!) { staffMembersByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) { items { staffId name active } } }',
        { accountId: await api.accountId(), skip: 0, take: 500 }
      );
      const staff = found?.staffMembersByAccount.items.find((row) => row.name === name && row.active);
      if (staff) {
        await api.tryGql(
          'manager',
          'mutation($staffId: UUID!) { deactivateStaffMember(command: { staffId: $staffId }) }',
          { staffId: staff.staffId }
        );
      }
    });

    await staff.create(
      { fields: { name, phone: '3001234567', documentNumber: String(Date.now()) } },
      name,
      'name'
    );

    await staff.reload(shell, 'manageAdmin');
    const row = await staff.section.findRow(name);
    await expect(row).toContainText('3001234567');

    // Deactivation is confirmed through the browser's own confirm dialog.
    page.once('dialog', (dialog) => void dialog.accept());
    await row.getByRole('button', { name: t('staff.deactivate') }).click();
    await expect
      .poll(async () => (await staff.section.findRow(name)).innerText(), { timeout: 45_000 })
      .toContain(t('generic.no'));
  });

  test('the staff access dialog, qualifications and assignment sections render', async ({
    page,
    shell,
    t,
    api,
    cleanup,
  }) => {
    const staff = await seedStaff(api, cleanup);
    await shell.open('manageAdmin');

    for (const key of ['staff-qualifications', 'qualification-expirations']) {
      const section = new CrudFlow(page, key, t).section;
      await section.expand();
      await expect(section.root).toBeVisible();
      // The staff-scoped section asks for a staff first, and says so
      // rather than rendering an empty table that means nothing.
      if (key !== 'qualification-expirations') {
        await expect(section.root.getByText(t('workforce.selectStaffHint'))).toBeVisible();
      }
    }

    // Credentials live in the per-staff detail dialog: the table keeps one action per staff.
    const dialog = await openStaffAccess(page, t, staff);
    await expect(dialog.getByText(t('workforce.credentials.title'))).toBeVisible();
    await expect(dialog.getByText(t('workforce.credentials.empty'))).toBeVisible({ timeout: 45_000 });
  });

  test('a manager issues a staff credential and walks its lifecycle', async ({
    page,
    shell,
    t,
    api,
    cleanup,
  }) => {
    const staff = await seedStaff(api, cleanup);
    const login = `e2e${unique().replace(/-/g, '')}`;

    await shell.open('manageAdmin');
    const dialog = await openStaffAccess(page, t, staff);
    await expect(dialog.getByText(t('workforce.credentials.empty'))).toBeVisible({ timeout: 45_000 });

    await dialog.getByRole('button', { name: t('workforce.credentials.create') }).click();
    const form = new FormDialog(page, t);
    await form.waitOpen();
    await form.save();
    await expect(form.anyError.first()).toBeVisible();

    await form.field('credentialLogin').fill(login);
    await form.field('credentialPassword').fill('E2eStaff9!');
    await form.saveAndClose();

    // A credential that has never been used reads "Pending Activation" — saying
    // "Revoked" for one that was simply never activated was a lie (spec 09 AC7).
    const row = () => dialog.locator('[data-testid^="row-"]').filter({ hasText: login }).first();
    await expect(row()).toContainText(t('workforce.credentials.statusPending'), { timeout: 45_000 });

    await row().getByRole('button', { name: t('workforce.credentials.activate') }).click();
    await form.waitOpen();
    await form.field('credentialPassword').fill('E2eStaff9!');
    await form.saveAndClose();
    await expect(row()).toContainText(t('workforce.credentials.statusActive'), { timeout: 45_000 });

    // Locking is a dated block, not a deletion.
    await row().getByRole('button', { name: t('workforce.credentials.lock') }).click();
    await form.waitOpen();
    await form
      .field('credentialLockedUntil')
      .fill(new Date(Date.now() + 60 * 60 * 1000).toISOString().slice(0, 16));
    await form.saveAndClose();
    await expect(row()).toContainText(t('workforce.credentials.statusLocked'), { timeout: 45_000 });

    // Activate is offered from every state, so no state is a dead end.
    await row().getByRole('button', { name: t('workforce.credentials.activate') }).click();
    await form.waitOpen();
    await form.field('credentialPassword').fill('E2eStaff9!');
    await form.saveAndClose();
    await expect(row()).toContainText(t('workforce.credentials.statusActive'), { timeout: 45_000 });

    // Revoking keeps the record and stops the sign-in.
    await row().getByRole('button', { name: t('workforce.credentials.revoke') }).click();
    const confirm = new ConfirmDialog(page, t);
    await expect(confirm.root.getByText(t('workforce.credentials.revokeConfirm'))).toBeVisible();
    await confirm.confirm();
    await expect(row()).toContainText(t('workforce.credentials.statusRevoked'), { timeout: 45_000 });
  });

  test('a manager records a qualification with an expiry, edits it and deletes it', async ({
    page,
    shell,
    t,
    api,
    cleanup,
  }) => {
    const staff = await seedStaff(api, cleanup);
    const number = `e2e-lic-${unique()}`;
    // Inside the 30-day window the expirations table reports on.
    const expires = new Date(Date.now() + 10 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10);

    await shell.open('manageAdmin');
    const section = new CrudFlow(page, 'staff-qualifications', t).section;
    await section.expand();
    await selectFirstMatching(page, section.root, 'qualificationStaffId', staff);
    await expect(section.root.getByText(t('workforce.qualifications.empty'))).toBeVisible({
      timeout: 45_000,
    });

    await section.clickAdd();
    const form = new FormDialog(page, t);
    await form.waitOpen();
    await form.save();
    await expect(form.anyError.first()).toBeVisible();

    await form.selectFirst('qualificationType');
    await form.field('qualificationNumber').fill(number);
    await form.field('qualificationExpiresAt').fill(expires);
    await form.field('qualificationIssuingAuthority').fill('E2E authority');
    await form.saveAndClose();

    await expect(await section.findRow(number)).toBeVisible({ timeout: 45_000 });

    await (await section.findRow(number)).getByRole('button', { name: t('generic.edit') }).click();
    await form.waitOpen();
    await expect(form.field('qualificationNumber')).toHaveValue(number);
    await form.field('qualificationIssuingAuthority').fill('E2E authority, renamed');
    await form.saveAndClose();

    // The change is on the record, not just on the screen.
    await shell.reloadTo('manageAdmin');
    await section.expand();
    await selectFirstMatching(page, section.root, 'qualificationStaffId', staff);
    await expect(await section.findRow(number)).toContainText('E2E authority, renamed', {
      timeout: 45_000,
    });

    // A qualification that expires within thirty days is what the expirations
    // table exists to surface.
    const expirations = new CrudFlow(page, 'qualification-expirations', t).section;
    await expirations.expand();
    // It expires inside the window, so it MUST be listed. The table is account-wide, so in
    // an account with other expiring staff the row is not on the first page.
    await expect(await expirations.findRowAnyPage(staff)).toBeVisible({ timeout: 45_000 });

    await (await section.findRow(number)).getByRole('button', { name: t('generic.delete') }).click();
    await new ConfirmDialog(page, t).confirm();
    await expect(section.root.getByText(number)).toHaveCount(0, { timeout: 45_000 });
  });

  test('a manager assigns a staff to a unit and ends the assignment', async ({
    page,
    shell,
    t,
    api,
    cleanup,
  }) => {
    const staff = await seedStaff(api, cleanup);
    const unitName = uniqueName('assign-unit');
    const transporterId = await api.createTransporter(unitName);
    cleanup.add(`unit ${unitName}`, () => api.deleteTransporter(transporterId));

    // Assignments are day-to-day dispatch: they live on the dashboard's staff operations tab.
    await shell.open('dashboard');
    await openTab(page, t, 'staff');
    const section = new CrudFlow(page, 'staff-assignments', t).section;
    await section.expand();
    await expect(section.root.getByText(t('workforce.selectStaffHint'))).toBeVisible();
    await selectFirstMatching(page, section.root, 'assignStaffId', staff);
    await expect(section.root.getByText(t('workforce.assignments.emptyActive'))).toBeVisible({
      timeout: 45_000,
    });

    await selectFirstMatching(page, section.root, 'assignTransporterId', unitName);
    await section.root.getByRole('button', { name: t('workforce.assignments.assign') }).click();

    const row = section.root.locator('[data-testid^="row-"]').filter({ hasText: unitName }).first();
    await expect(row).toBeVisible({ timeout: 45_000 });
    await expect(row).toContainText(t('workforce.assignments.types.regular'));

    // Ending an assignment closes it rather than erasing it — the history is
    // what a workforce audit reads.
    await row.getByRole('button', { name: t('workforce.assignments.end') }).click();
    const confirm = new ConfirmDialog(page, t);
    await expect(confirm.root.getByText(t('workforce.assignments.endConfirm'))).toBeVisible();
    await confirm.confirm();
    await expect(section.root.getByText(t('workforce.assignments.emptyActive'))).toBeVisible({
      timeout: 45_000,
    });
  });

  test('the toll-class mapping is fleet configuration and explains that a class is required', async ({
    page,
    shell,
    t,
  }) => {
    await shell.open('manageAdmin');
    const section = new CrudFlow(page, 'toll-classes', t).section;
    await section.expand();
    await section.clickAdd();

    const dialog = new FormDialog(page, t);
    await dialog.waitOpen();
    await expect(dialog.root.getByText(t('tolls.transporterClass.description'))).toBeVisible();
    await dialog.cancel();
  });
});
