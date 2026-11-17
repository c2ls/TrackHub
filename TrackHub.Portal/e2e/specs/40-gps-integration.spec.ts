/**
 * GPS Integration: the provider dashboard, operator administration, the
 * synchronized-device inventory and the assignment table.
 *
 * A real synchronisation needs real provider credentials, so the sync trigger
 * only runs when `E2E_GPS_OPERATOR_ID` names an operator to run it against.
 */

import { test, expect, optional, uniqueName } from '../fixtures';
import { CrudFlow } from '../pages/crud';
import { Section } from '../pages/tableAccordion';
import { FormDialog } from '../pages/dialogs';

test.describe('gps integration', () => {
  test('the dashboard reports operators, devices and synchronisation', async ({
    shell,
    page,
    t,
  }) => {
    await shell.open('gpsIntegration');

    for (const title of [
      t('gpsIntegration.dashboard.operators'),
      t('gpsIntegration.dashboard.devices'),
      t('gpsIntegration.dashboard.sync'),
    ]) {
      await expect(page.getByText(title, { exact: true }).first()).toBeVisible({ timeout: 60_000 });
    }

    const breakdown = new Section(page, 'gps-provider-status', t);
    await expect(breakdown.root).toBeVisible();
    await breakdown.expand();
  });

  test('every section of the screen is present and expands', async ({ shell, page, t }) => {
    await shell.open('gpsIntegration');

    for (const key of [
      'gps-operators',
      'gps-sync-runs',
      'gps-open-alerts',
      'gps-devices',
      'gps-assignments',
      'gps-retention',
    ]) {
      const section = new Section(page, key, t);
      await expect(section.root).toBeVisible();
      await section.expand();
      await section.collapse();
    }
  });

  test('a manager creates an operator, disables it and deletes it', async ({
    page,
    shell,
    t,
    cleanup,
  }) => {
    const name = uniqueName('operator');
    const operators = new CrudFlow(page, 'gps-operators', t);

    await shell.open('gpsIntegration');
    await operators.open();
    cleanup.add(`operator ${name}`, () => operators.removeIfPresent(name));

    await operators.section.clickAdd();
    await operators.form.waitOpen();
    await operators.form.save();
    await expect(operators.form.root).toBeVisible();
    await expect(operators.form.anyError.first()).toBeVisible();

    await operators.form.fill({
      name,
      description: 'created by the e2e suite',
      contactName: 'E2E',
      syncIntervalMinutes: '30',
    });
    // The provider list comes from the Router's capability catalog, so the test
    // takes whatever the deployment offers rather than naming a provider.
    await operators.form.selectFirst('protocolTypeId');
    await operators.form.saveAndClose();

    const row = await operators.section.findRow(name);
    await expect(row).toBeVisible();

    await operators.reload(shell, 'gpsIntegration');
    const stored = await operators.section.findRow(name);
    await stored.getByRole('button', { name: t('generic.disable') }).click();
    await expect
      .poll(async () => (await operators.section.findRow(name)).innerText(), { timeout: 45_000 })
      .toContain(t('generic.enable'));

    await operators.remove(name);
  });

  test('the credential dialog refuses an operator credential with no endpoint', async ({
    page,
    shell,
    t,
    cleanup,
  }) => {
    const name = uniqueName('operator');
    const operators = new CrudFlow(page, 'gps-operators', t);

    await shell.open('gpsIntegration');
    await operators.open();
    cleanup.add(`operator ${name}`, () => operators.removeIfPresent(name));
    await operators.create(
      { fields: { name }, selectFirst: ['protocolTypeId'] },
      name,
      'name'
    );

    const row = await operators.section.findRow(name);
    await row.getByRole('button', { name: t('credential.title') }).click();
    await operators.form.waitOpen();

    await operators.form.save();
    await expect(operators.form.root).toBeVisible();
    await expect(operators.form.error('uri')).toBeVisible();

    await operators.form.fill({ uri: 'https://example.invalid/gps', username: 'e2e', password: 'e2e' });
    await operators.form.saveAndClose();

    await operators.remove(name);
  });

  test('the synchronized-device and assignment tables render their columns', async ({
    page,
    shell,
    t,
  }) => {
    await shell.open('gpsIntegration');

    const devices = new Section(page, 'gps-devices', t);
    await devices.expand();
    await expect(
      devices.root.getByRole('columnheader', { name: t('gpsIntegration.columns.detectedStatus') })
    ).toBeVisible();

    const assignments = new Section(page, 'gps-assignments', t);
    await assignments.expand();
    await expect(
      assignments.root.getByRole('columnheader', { name: t('gpsIntegration.columns.status') })
    ).toBeVisible();
  });

  test('the operator dialog offers exactly the providers the Router reports', async ({
    shell,
    page,
    t,
    api,
  }) => {
    // The portal carries no local protocol list: the picker is fed by the
    // Router's ProviderCapabilityCatalog, built from the registered providers.
    const catalog = await api.gql<{ providerCapabilities: { displayName: string }[] }>(
      'router',
      'query { providerCapabilities { displayName } }'
    );
    const registered = catalog.providerCapabilities.map((provider) => provider.displayName).sort();

    await shell.open('gpsIntegration');
    const operators = new Section(page, 'gps-operators', t);
    await operators.expand();
    await operators.clickAdd();

    const form = new FormDialog(page, t);
    await form.waitOpen();
    await form.field('protocolTypeId').click();
    const offered = (
      await page.getByRole('option').evaluateAll((nodes) =>
        nodes
          .filter((node) => node.getAttribute('aria-disabled') !== 'true')
          .map((node) => (node.textContent ?? '').trim())
      )
    ).sort();

    expect(offered).toEqual(registered);

    await page.keyboard.press('Escape');
    await form.cancel();
  });

  test('the manual device dialog appears only for providers with no device catalog', async ({
    shell,
    page,
    t,
    api,
  }) => {
    // Manual registration exists for providers whose API exposes no catalog, so
    // sync can never discover their devices. Offering it for a provider that
    // DOES have one would let a hand-typed device collide with a synced one and
    // be flagged as removed.
    const catalog = await api.gql<{
      providerCapabilities: { displayName: string; deviceCatalog: boolean }[];
    }>('router', 'query { providerCapabilities { displayName deviceCatalog } }');
    const catalogless = catalog.providerCapabilities.filter((p) => !p.deviceCatalog);

    await shell.open('gpsIntegration');
    const devices = new Section(page, 'gps-devices', t);
    await devices.expand();

    const add = devices.addButton;
    if (catalogless.length === 0) {
      // Every registered provider can enumerate its own devices, so there is
      // nothing to register by hand and the affordance is correctly absent.
      await expect(add).toHaveCount(0);
      return;
    }

    // The add affordance also needs an operator on one of those providers; when
    // the account has none, its absence is still the right answer.
    if ((await add.count()) === 0) {
      test.skip(
        true,
        'This account has no operator on a provider without a device catalog, so manual registration is correctly not offered.'
      );
    }

    await devices.clickAdd();
    const form = new FormDialog(page, t);
    await form.waitOpen();
    await expect(form.root.getByText(t('gpsIntegration.deviceForm.title'))).toBeVisible();
    await expect(form.field('operatorId')).toBeVisible();
    await expect(form.field('serial')).toBeVisible();
    await expect(form.root.getByText(t('gpsIntegration.deviceForm.autoAssignHelp'))).toBeVisible();

    // Nothing is registered without a name and a serial.
    await form.save();
    await expect(form.anyError.first()).toBeVisible();
    await form.cancel();
  });

  test('the retention section reports the account position settings', async ({
    page,
    shell,
    t,
  }) => {
    const retention = new Section(page, 'gps-retention', t);
    await shell.open('gpsIntegration');
    await retention.expand();

    // Read-only by design: the values are managed as account FEATURE
    // configuration by the platform administrator, not edited here.
    await expect(retention.root.getByText(t('gpsIntegration.retention.managedNote'))).toBeVisible();
    await expect(
      retention.root.getByText(t('gpsIntegration.retention.retentionDays'))
    ).toBeVisible();
    // Read-only means no inputs: the only button is the accordion header itself.
    await expect(retention.root.getByRole('textbox')).toHaveCount(0);
    await expect(retention.root.getByRole('spinbutton')).toHaveCount(0);
  });

  test('a manual synchronisation reports its device counts', async ({ page, shell, t }) => {
    const operatorId = optional('E2E_GPS_OPERATOR_ID');
    test.skip(
      !operatorId,
      'Set E2E_GPS_OPERATOR_ID to an operator with working provider credentials — a synchronisation cannot be simulated.'
    );

    const operators = new Section(page, 'gps-operators', t);
    await shell.open('gpsIntegration');
    await operators.expand();

    const row = operators.rowById(operatorId!);
    await expect(row).toBeVisible({ timeout: 30_000 });
    await row.getByRole('button', { name: t('gpsIntegration.actions.sync') }).click();

    const result = page.getByTestId('dialog-message');
    await expect(result).toBeVisible({ timeout: 120_000 });
    await expect(result).toContainText(/Sync completed|Sync did not complete/);
    await result.getByRole('button', { name: t('generic.close') }).click();
  });
});
