import { test, expect, uniqueName } from '../fixtures';
import { CrudFlow } from '../pages/crud';

interface Provider {
  protocol: string;
  displayName: string;
  minPositionIntervalSeconds: number;
  credentialFields: string[];
  defaultUri: string | null;
}

test.describe('gps providers', () => {
  test('the Router reports each provider’s polling limit and credential fields', async ({ api }) => {
    const data = await api.gql<{ providerCapabilities: Provider[] }>(
      'router',
      'query { providerCapabilities { protocol displayName minPositionIntervalSeconds credentialFields defaultUri } }'
    );
    const byProtocol = new Map(data.providerCapabilities.map((p) => [p.protocol, p]));

    expect(byProtocol.get('RedGps')).toMatchObject({
      displayName: 'RedGPS',
      minPositionIntervalSeconds: 60,
      credentialFields: ['USERNAME', 'PASSWORD', 'KEY'],
      defaultUri: 'https://api.service24gps.com/api/v1',
    });
    expect(byProtocol.get('Motive')).toMatchObject({ minPositionIntervalSeconds: 0, credentialFields: ['KEY'] });
    expect(byProtocol.get('Webfleet')?.minPositionIntervalSeconds).toBe(15);
    expect(byProtocol.get('Plaspy')?.minPositionIntervalSeconds).toBe(120);
    expect(byProtocol.get('Flespi')?.minPositionIntervalSeconds).toBe(10);
    for (const protocol of ['Tracksolid', 'Cartrack', 'Pegasus']) {
      expect(byProtocol.has(protocol)).toBe(true);
    }
  });

  test('a limited provider shows its limit, and its credential form asks only for its own fields', async ({
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
    await operators.form.fill({ name, syncIntervalMinutes: '30' });
    await operators.form.select('protocolTypeId', 'RedGPS');
    await expect(operators.form.root.getByText(t('operator.providerPositionLimit', { seconds: 60 }))).toBeVisible();
    await operators.form.saveAndClose();

    const row = await operators.section.findRow(name);
    await row.getByRole('button', { name: t('credential.title') }).click();
    await operators.form.waitOpen();

    await expect(operators.form.field('uri')).toHaveValue('https://api.service24gps.com/api/v1');
    await expect(operators.form.root.getByLabel(t('credential.providers.RedGps.key'))).toBeVisible();
    await expect(operators.form.field('username')).toBeVisible();
    await expect(operators.form.field('password')).toBeVisible();
    await expect(operators.form.field('key2')).toHaveCount(0);

    await operators.form.fill({ username: 'e2e', password: 'e2e', key: 'e2e-key' });
    await operators.form.saveAndClose();

    await (await operators.section.findRow(name)).getByRole('button', { name: t('credential.title') }).click();
    await operators.form.waitOpen();
    await expect(operators.form.field('uri')).toHaveValue('https://api.service24gps.com/api/v1');
    await expect(operators.form.field('username')).toHaveValue('e2e');
    await operators.form.cancel();

    await operators.remove(name);
  });

  test('a key-only provider asks for nothing but the URL and its key', async ({ page, shell, t, cleanup }) => {
    const name = uniqueName('operator');
    const operators = new CrudFlow(page, 'gps-operators', t);

    await shell.open('gpsIntegration');
    await operators.open();
    cleanup.add(`operator ${name}`, () => operators.removeIfPresent(name));

    await operators.section.clickAdd();
    await operators.form.waitOpen();
    await operators.form.fill({ name, syncIntervalMinutes: '30' });
    await operators.form.select('protocolTypeId', 'Motive');
    await expect(operators.form.root.getByText(/one position update every/i)).toHaveCount(0);
    await operators.form.saveAndClose();

    await (await operators.section.findRow(name)).getByRole('button', { name: t('credential.title') }).click();
    await operators.form.waitOpen();

    await expect(operators.form.field('uri')).toHaveValue('https://api.gomotive.com');
    await expect(operators.form.root.getByLabel(t('credential.providers.Motive.key'))).toBeVisible();
    for (const hidden of ['username', 'password', 'key2']) {
      await expect(operators.form.field(hidden)).toHaveCount(0);
    }
    await operators.form.cancel();

    await operators.remove(name);
  });
});
