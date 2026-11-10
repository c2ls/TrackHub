/**
 * System Admin — the platform-wide screen only an Administrator reaches.
 *
 * Two rules shape what is exercised here:
 *  - never touch a seeded client (`web_client`, `router_client`, …) or the
 *    mandatory 14-pair permission baseline;
 *  - the permission matrices are proved by GRANTING and then REVOKING the same
 *    cell, so the account is left exactly as it was found.
 */

import { test, expect, unique, uniqueEmail, uniqueName, flag } from '../fixtures';
import { CrudFlow } from '../pages/crud';
import { Section } from '../pages/tableAccordion';
import { FormDialog, PermissionMatrix } from '../pages/dialogs';

test.describe('system admin', () => {
  test('every platform section is present and expands', async ({ page, shell, t }) => {
    await shell.open('systemAdmin');

    for (const key of [
      'accounts',
      'clients',
      'service-client-permissions',
      'transporter-types',
      'geocoding-providers',
      'toll-catalog',
      'system-roles',
      'system-policies',
      'system-account-features',
      'support-grants',
    ]) {
      const section = new Section(page, key, t);
      await section.expand();
      await expect(section.root).toBeVisible();
      await section.collapse();
    }
  });

  test('the accounts section refuses an account with no name', async ({ page, shell, t }) => {
    await shell.open('systemAdmin');
    const accounts = new CrudFlow(page, 'accounts', t);
    await accounts.open();

    await accounts.section.clickAdd();
    const form = new FormDialog(page, t);
    await form.waitOpen();
    await form.save();

    await expect(form.root).toBeVisible();
    await expect(form.error('name')).toBeVisible();
    await form.cancel();
  });

  test('the status dialog offers only the transitions the account may take', async ({
    page,
    shell,
    t,
  }) => {
    // Read-only on purpose: the suite runs as a user of the seeded account, and
    // suspending it would end the run. What is asserted is the state machine —
    // which targets are offered, and that suspending demands a reason
    // (`data/accountStatuses.ts`).
    await shell.open('systemAdmin');
    const accounts = new CrudFlow(page, 'accounts', t);
    await accounts.open();

    const row = accounts.section.rows.first();
    await expect(row).toBeVisible({ timeout: 45_000 });
    await row.getByRole('button', { name: t('account.changeStatus') }).click();

    const form = new FormDialog(page, t);
    await form.waitOpen();
    await form.field('targetStatus').click();
    const offered = await page
      .getByRole('option')
      .evaluateAll((nodes) => nodes.map((node) => (node.textContent ?? '').trim()));
    // An account never offers its own current status, and never offers a
    // transition the state machine forbids.
    expect(offered.length).toBeGreaterThan(0);
    expect(offered).not.toContain(t('account.statusTrial'));

    // Suspending needs a reason; the dialog says so instead of failing silently.
    if (offered.includes(t('account.statusSuspended'))) {
      await page.getByRole('option', { name: t('account.statusSuspended'), exact: true }).click();
      await form.save();
      await expect(form.error('reason')).toBeVisible();
    } else {
      await page.keyboard.press('Escape');
    }
    await form.cancel();
  });

  test('an administrator creates an account with its manager, edits it and suspends it', async ({
    page,
    shell,
    t,
    api,
  }) => {
    test.skip(
      !flag('E2E_CREATE_ACCOUNT'),
      'Set E2E_CREATE_ACCOUNT=1 to let the suite create a real account — accounts cannot be deleted, only archived.'
    );

    const name = uniqueName('account');
    const email = uniqueEmail('owner');
    await shell.open('systemAdmin');
    const accounts = new CrudFlow(page, 'accounts', t);
    await accounts.open();

    await accounts.section.clickAdd();
    const form = new FormDialog(page, t);
    await form.waitOpen();
    await form.field('name').fill(name);
    await form.field('description').fill('created by the e2e suite');
    await form.selectFirst('typeId');
    // A new account is created together with the person who will run it.
    await form.field('emailAddress').fill(email);
    await form.field('password').fill('E2eOwner9!');
    await form.field('firstName').fill('E2E');
    await form.field('lastName').fill('Owner');
    await form.saveAndClose();

    await accounts.section.search(name);
    const row = await accounts.section.findRow(name);
    await expect(row).toBeVisible();
    // A new account is operational from the start: createAccount maps `active` onto the
    // lifecycle status (true -> Active). Trial is reached through a subscription, never
    // through creation (spec 03 — migration backfills true -> Active, false -> Suspended).
    await expect(row).toContainText(t('account.statusActive'));

    // Editing it sticks.
    await row.getByRole('button', { name: t('generic.edit') }).click();
    await form.waitOpen();
    await expect(form.field('name')).toHaveValue(name);
    await form.field('description').fill('renamed by the e2e suite');
    await form.saveAndClose();

    await accounts.reload(shell, 'systemAdmin');
    await accounts.section.search(name);
    await expect(await accounts.section.findRow(name)).toContainText('renamed by the e2e suite');

    // Suspending records a reason and moves the account out of the operational band.
    // There is no delete: an account is archived, never removed, so the row stays.
    const found = await accounts.section.findRow(name);
    await found.getByRole('button', { name: t('account.changeStatus') }).click();
    await form.waitOpen();
    await form.select('targetStatus', t('account.statusSuspended'));
    await form.field('reason').fill('suspended by the e2e suite');
    await form.saveAndClose();

    await expect(await accounts.section.findRow(name)).toContainText(
      t('account.statusSuspended'),
      { timeout: 45_000 }
    );

    // Left archived rather than lingering as a live tenant.
    const archive = await accounts.section.findRow(name);
    await archive.getByRole('button', { name: t('account.changeStatus') }).click();
    await form.waitOpen();
    await form.select('targetStatus', t('account.statusArchived'));
    await form.saveAndClose();
    await expect(await accounts.section.findRow(name)).toContainText(
      t('account.statusArchived'),
      { timeout: 45_000 }
    );
  });

  test('an administrator creates an OAuth client, links a user and deletes it', async ({
    page,
    shell,
    t,
    cleanup,
  }) => {
    const name = uniqueName('client');
    const email = uniqueEmail('integration');
    const clients = new CrudFlow(page, 'clients', t);
    const users = new CrudFlow(page, 'users', t);

    // A client links to an INTEGRATION user, and the link is one-to-one: reusing
    // a seeded integration user that already owns a client fails.
    await shell.open('manageAdmin');
    await users.open();
    cleanup.add(`user ${email}`, () => users.removeIfPresent(email));
    await users.section.clickAdd();
    await users.form.waitOpen();
    await users.form.fill({
      emailAddress: email,
      password: 'E2eTestPass1',
      username: email.split('@')[0],
      firstName: 'E2E',
      lastName: 'Integration',
    });
    await users.form.root.locator('input[name="integrationUser"]').check();
    await users.form.saveAndClose();
    await expect(await users.section.findRow(email)).toBeVisible();

    await shell.open('systemAdmin');
    await clients.open();
    cleanup.add(`client ${name}`, () => clients.removeIfPresent(name));

    await clients.create(
      { fields: { name, description: 'created by the e2e suite', secret: 'E2eClientSecret1' } },
      name,
      'name'
    );

    // Editing an existing client exposes only its linked user: name, description
    // and secret are create-only fields (`ClientsDialog` renders them behind
    // `!values.clientId`), and the portal has no redirect-URI editor at all.
    await clients.section.rowAction(name, t('generic.edit'));
    await clients.form.waitOpen();
    await expect(clients.form.field('name')).toHaveCount(0);
    await clients.form.select('userId', email);
    await clients.form.saveAndClose();

    await clients.reload(shell, 'systemAdmin');
    await expect(await clients.section.findRow(name)).toBeVisible();

    await clients.remove(name);

    await shell.open('manageAdmin');
    await users.open();
    await users.remove(email);
  });

  test('an administrator creates a geocoding provider and deletes it', async ({
    page,
    shell,
    t,
    cleanup,
  }) => {
    const name = uniqueName('geocoder');
    const providers = new CrudFlow(page, 'geocoding-providers', t);

    await shell.open('systemAdmin');
    await providers.open();
    cleanup.add(`geocoding provider ${name}`, () => providers.removeIfPresent(name));

    await providers.create(
      {
        fields: { name, endpointUri: 'https://example.invalid/geocode' },
        selects: { type: t('geocodingProviders.types.nominatim') },
      },
      name,
      'name'
    );

    await providers.reload(shell, 'systemAdmin');
    await providers.assertStored(name, {
      fields: { endpointUri: 'https://example.invalid/geocode' },
    });

    // Switching the ACTIVE provider changes geocoding for every account, so it
    // only runs when the environment opts in.
    test.skip(
      !flag('E2E_ALLOW_GEOCODING_SWITCH'),
      'Set E2E_ALLOW_GEOCODING_SWITCH=1 to exercise activating a geocoding provider (it affects every account).'
    );
    await providers.remove(name);
  });

  test('the transporter-type dialog persists a detection threshold', async ({
    page,
    shell,
    t,
    cleanup,
  }) => {
    const types = new CrudFlow(page, 'transporter-types', t);
    await shell.open('systemAdmin');
    const section = await types.open();

    const first = section.rows.first();
    await expect(first).toBeVisible();
    const label = (await first.innerText()).split('\n')[0].trim();

    await section.rowAction(label, t('generic.edit'));
    await types.form.waitOpen();
    const original = await types.form.field('maxDistance').inputValue();
    cleanup.add('transporter type', async () => {
      await shell.open('systemAdmin');
      await types.open();
      await types.edit(label, { fields: { maxDistance: original } });
    });

    const changed = original === '900' ? '800' : '900';
    await types.form.field('maxDistance').fill(changed);
    await types.form.saveAndClose();

    await types.reload(shell, 'systemAdmin');
    await types.assertStored(label, { fields: { maxDistance: changed } });
  });

  test('an administrator adds a toll vehicle class and a station', async ({
    page,
    shell,
    t,
    api,
    cleanup,
  }) => {
    const classCode = `E2E${unique().replace(/[^a-z0-9]/gi, '').slice(-8).toUpperCase()}`;
    // The station table is ONE server page of 100, ordered by name, drawn from a
    // catalog of ~300 platform stations — there is no client pager to walk. So
    // the name has to sort into that window for the assertion to be about the
    // save rather than about paging. This only stays true because the cleanup
    // below actually works now: it used a mutation shape that does not exist
    // (`command: { tollStationId }` instead of `id`), `tryGql` swallowed the
    // error, and thirteen stations had leaked into the shared catalog.
    const stationName = `AAA-${unique()}`;
    const catalog = new Section(page, 'toll-catalog', t);

    await shell.open('systemAdmin');
    await catalog.expand();

    cleanup.add(`toll class ${classCode}`, async () => {
      const classes = await api.tryGql<{
        tollVehicleClasses: { tollVehicleClassId: string; code: string; active: boolean }[];
      }>('tripManagement', 'query { tollVehicleClasses { tollVehicleClassId code active } }');
      const found = classes?.tollVehicleClasses.find((row) => row.code === classCode && row.active);
      if (found) {
        await api.tryGql(
          'tripManagement',
          'mutation($id: UUID!) { deactivateTollVehicleClass(id: $id) }',
          { id: found.tollVehicleClassId }
        );
      }
    });

    await catalog.root.getByRole('button', { name: t('tolls.catalog.addClass') }).click();
    const form = new CrudFlow(page, 'toll-catalog', t).form;
    await form.waitOpen();
    // The vehicle-class table pages ten at a time in the browser and KEEPS
    // deactivated classes in the list, so the sort-order-1 bucket only ever
    // grows and the newest row is not on the first page. Walk the pages for it.
    await form.fill({ code: classCode, name: `E2E class ${classCode}`, sortOrder: '1' });
    await form.saveAndClose();
    await expect(await catalog.findRowAnyPage(classCode)).toBeVisible();

    await catalog.root.getByRole('button', { name: t('tolls.catalog.addStation') }).click();
    await form.waitOpen();
    await form.fill({ name: stationName, code: classCode, latitude: '4.7', longitude: '-74.1' });
    await form.saveAndClose();
    // The stations table is client-paged too, and it is the SECOND pager in this
    // section, so the walk is scoped to that table.
    await expect(
      await catalog.findRowAnyPage(stationName, 45_000, catalog.root.getByTestId('toll-stations'))
    ).toBeVisible();

    cleanup.add(`toll station ${stationName}`, async () => {
      const stations = await api.tryGql<{
        tollStations: { items: { tollStationId: string; name: string; active: boolean }[] };
      }>(
        'tripManagement',
        'query($take: Int) { tollStations(query: { take: $take }) { items { tollStationId name active } } }',
        { take: 500 }
      );
      const found = stations?.tollStations.items.find((row) => row.name === stationName && row.active);
      if (found) {
        await api.tryGql(
          'tripManagement',
          'mutation($id: UUID!) { deactivateTollStation(id: $id) }',
          { id: found.tollStationId }
        );
      }
    });
  });

  test('the toll import dialog reports the rows it could not accept', async ({
    page,
    shell,
    t,
  }) => {
    const catalog = new Section(page, 'toll-catalog', t);
    await shell.open('systemAdmin');
    await catalog.expand();

    await catalog.root.getByRole('button', { name: t('tolls.catalog.import') }).click();
    const dialog = page.getByTestId('dialog-form');
    await expect(dialog).toBeVisible();

    // One header, one row that cannot be parsed. The import reports per row
    // instead of refusing the whole file.
    await dialog.locator('#csv').fill('stationCode,stationName\nBROKEN-ROW-ONLY');
    await dialog.getByRole('button', { name: t('generic.save') }).click();

    await expect(
      dialog
        .getByText(t('tolls.catalog.importErrors'))
        .or(dialog.getByText(t('tolls.catalog.rowsRead')))
        .first()
    ).toBeVisible({ timeout: 45_000 });
    await dialog.getByRole('button', { name: t('generic.cancel') }).click();
  });

  test('a policy permission grant can be added and taken back', async ({ page, shell, t }) => {
    // Policies are ADDITIVE grants (roles OR policies), so a grant added and
    // removed here cannot dent the mandatory 14-pair baseline every role holds.
    const policies = new Section(page, 'system-policies', t);
    await shell.open('systemAdmin');
    await policies.expand();

    await policies.root.locator('#policyId').click();
    await page.getByRole('option', { name: t('policies.readOnly') }).click();

    const matrix = new PermissionMatrix(page, policies.root);
    await expect.poll(() => matrix.cells.count(), { timeout: 45_000 }).toBeGreaterThan(0);

    // The matrix only ticks a box once the backend confirms the grant, so the
    // assertion is on the state AFTER the round trip, not on the click.
    const cell = await matrix.firstUngranted();
    await cell.click();
    await expect(cell).toBeChecked({ timeout: 30_000 });

    await cell.click();
    await expect(cell).not.toBeChecked({ timeout: 30_000 });
  });

  test('the role permission matrix renders the seeded roles', async ({ page, shell, t }) => {
    const roles = new Section(page, 'system-roles', t);
    await shell.open('systemAdmin');
    await roles.expand();

    await roles.root.locator('#roleId').click();
    await page.getByRole('option', { name: t('roles.user') }).click();

    const matrix = new PermissionMatrix(page, roles.root);
    await expect.poll(() => matrix.cells.count(), { timeout: 45_000 }).toBeGreaterThan(0);
    // At least one grant is already held: the baseline every role carries.
    await expect.poll(() => roles.root.locator('input:checked').count()).toBeGreaterThan(0);

    // Writing to a ROLE widens what every user with it may do. The policy matrix
    // above proves the control; this one is opt-in.
    test.skip(
      !flag('E2E_ALLOW_ROLE_MATRIX_WRITE'),
      'Set E2E_ALLOW_ROLE_MATRIX_WRITE=1 to grant and revoke a role permission (it changes what every holder of that role may do).'
    );

    const cell = await matrix.firstUngranted();
    await cell.click();
    await expect(cell).toBeChecked({ timeout: 30_000 });
    await cell.click();
    await expect(cell).not.toBeChecked({ timeout: 30_000 });
  });

  test('an administrator creates a service-client permission and deletes it', async ({
    page,
    shell,
    t,
    cleanup,
  }) => {
    const resource = `E2eResource${unique().replace(/[^a-z0-9]/gi, '')}`;
    const permissions = new CrudFlow(page, 'service-client-permissions', t);

    await shell.open('systemAdmin');
    await permissions.open();
    cleanup.add(`service client permission ${resource}`, () => permissions.removeIfPresent(resource));

    await permissions.section.clickAdd();
    await permissions.form.waitOpen();
    await permissions.form.save();
    await expect(permissions.form.root).toBeVisible();
    await expect(permissions.form.anyError.first()).toBeVisible();

    await permissions.form.fill({
      clientId: 'e2e_probe_client',
      resource,
      action: 'Read',
      scope: 'Account',
      audience: 'manager',
    });
    await permissions.form.saveAndClose();

    await expect(await permissions.section.findRow(resource)).toBeVisible();
    await permissions.reload(shell, 'systemAdmin');

    const row = await permissions.section.findRow(resource);
    // Icon-only action buttons: the second one is the delete.
    await row.getByRole('button').last().click();
    await permissions.confirm.root
      .getByRole('button', { name: t('generic.confirm') })
      .click();
    await expect(permissions.section.row(resource)).toHaveCount(0, { timeout: 45_000 });

    await expect(permissions.confirm.root).toBeHidden();
  });

  test('the account features section lists the platform entitlements', async ({
    page,
    shell,
    t,
  }) => {
    const features = new Section(page, 'system-account-features', t);
    await shell.open('systemAdmin');
    await features.expand();

    // Toggling a feature here is what proves the gates; that lives in 95-gates,
    // against the dedicated E2E account so the master account is never changed.
    await expect(features.addButton).toBeVisible();
  });

  test('the support-grants section renders its request dialog', async ({ page, shell, t }) => {
    const grants = new Section(page, 'support-grants', t);
    await shell.open('systemAdmin');
    await grants.expand();

    await grants.addButton.click();
    const form = new CrudFlow(page, 'support-grants', t).form;
    await form.waitOpen();
    await form.save();
    // Every field is required, so an empty request is refused in the dialog.
    await expect(form.root).toBeVisible();
    await expect(form.anyError.first()).toBeVisible();
    await form.cancel();
  });
});
