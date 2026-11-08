/**
 * Account Management → Documents & Sharing: the document library, the expiration
 * dashboard, document-type configuration and public links.
 */

import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { test, expect, unique, uniqueName } from '../fixtures';
import { CrudFlow } from '../pages/crud';
import { Section } from '../pages/tableAccordion';
import { ConfirmDialog, FormDialog, pickOption } from '../pages/dialogs';

test.describe('account management — documents & sharing', () => {
  test('the document library renders its filters and its scan-status column', async ({
    page,
    shell,
    t,
  }) => {
    const library = new Section(page, 'documents-library', t);
    await shell.open('manageAdmin');
    await library.expand();

    await expect(library.root.locator('#filterCategory')).toBeVisible();
    await expect(library.root.locator('#filterStatus')).toBeVisible();
    await expect(
      library.root.getByRole('columnheader', { name: t('documentManagement.scanStatus') })
    ).toBeVisible();

    // Filtering by a category that cannot exist empties the list.
    await library.root.locator('#filterCategory').fill(`no-such-category-${unique()}`);
    await library.root.getByRole('button', { name: t('filters.search') }).click();
    await expect(library.rows).toHaveCount(0, { timeout: 30_000 });
  });

  test('the expiring-documents dashboard renders', async ({ page, shell, t }) => {
    const expiring = new Section(page, 'documents-expiring', t);
    await shell.open('manageAdmin');
    await expiring.expand();

    await expect(
      expiring.root.getByRole('columnheader', { name: t('documentManagement.expiresAt') })
    ).toBeVisible();
  });

  test('a manager configures a document type and disables it again', async ({
    page,
    shell,
    t,
    api,
    cleanup,
  }) => {
    const category = uniqueName('doctype');
    const types = new CrudFlow(page, 'document-types', t);

    await shell.open('manageAdmin');
    await types.open();
    cleanup.add(`document type ${category}`, async () => {
      const found = await api.tryGql<{
        documentTypes: { documentTypeId: string; displayName: string; category: string }[];
      }>(
        'manager',
        'query($accountId: UUID!) { documentTypes(query: { accountId: $accountId, includeDisabled: false }) { documentTypeId displayName category } }',
        { accountId: await api.accountId() }
      );
      const type = found?.documentTypes.find((row) => row.category === category);
      if (type) {
        await api.tryGql(
          'manager',
          'mutation($documentTypeId: UUID!) { disableDocumentType(command: { documentTypeId: $documentTypeId }) }',
          { documentTypeId: type.documentTypeId }
        );
      }
    });

    await types.section.clickAdd();
    await types.form.waitOpen();
    await types.form.save();
    await expect(types.form.root).toBeVisible();
    await expect(types.form.error('category')).toBeVisible();

    await types.form.fill({ category, displayName: category, defaultValidityDays: '90' });
    await types.form.saveAndClose();

    await expect(await types.section.findRowAnyPage(category)).toBeVisible();

    await types.reload(shell, 'manageAdmin');
    const stored = await types.section.findRowAnyPage(category);
    await expect(stored).toContainText('90');

    // A type is disabled, never deleted; the confirm dialog guards it.
    await stored.getByRole('button').first().click();
    await types.confirm.confirm();
    await expect
      .poll(async () => (await types.section.findRowAnyPage(category)).innerText(), {
        timeout: 45_000,
      })
      .toContain(t('generic.inactive'));
  });

  test('a manager uploads a document to a record, shares it and removes it', async ({
    page,
    shell,
    t,
    api,
    cleanup,
  }) => {
    // Documents are added from the panel embedded in the record they belong to,
    // not from the library — the library is the cross-account LIST and correctly
    // offers no Upload of its own. Today that panel is mounted on a staff's
    // qualifications, so that is where upload/version/share/void live.
    const staffName = uniqueName('staff');
    const staffId = await api.createStaffMember(staffName);
    cleanup.add(`staff ${staffName}`, () => api.deactivateStaffMember(staffId));

    // The upload dialog's category picker is fed by the account's document
    // types. An account whose types were all disabled can store no document at
    // all, so an enabled one is a precondition rather than the thing under test.
    const category = `e2e-type-${unique()}`;
    const documentTypeId = await api.ensureDocumentType(category);
    cleanup.add(`document type ${category}`, () => api.disableDocumentType(documentTypeId));

    const title = `E2E document ${unique()}`;
    const file = path.join(os.tmpdir(), `${title.replace(/\s+/g, '-')}.txt`);
    fs.writeFileSync(file, 'uploaded by the TrackHub e2e suite\n');

    await shell.open('manageAdmin');
    const qualifications = new Section(page, 'staff-qualifications', t);
    await qualifications.expand();
    await pickOption(page, qualifications.root.locator('#qualificationStaffId'), staffName, true);

    const upload = qualifications.root.getByRole('button', {
      name: t('documentManagement.upload'),
    });
    await expect(upload).toBeVisible({ timeout: 45_000 });
    await upload.click();

    const form = new FormDialog(page, t);
    await form.waitOpen();
    // Nothing is stored without a file. The drop area reports that itself, in
    // plain text beside the prompt, rather than through a field helper.
    await form.save();
    await expect(
      form.root.getByText(t('validation.required', { field: 'file' }))
    ).toBeVisible();

    await form.root.locator('input[type="file"]').setInputFiles(file);
    await expect(form.root.getByText(path.basename(file))).toBeVisible();
    // The panel loads the account's document types, so Type is a picker, not a text field.
    await form.select('category', category);
    await form.field('title').fill(title);
    await form.saveAndClose();

    const row = qualifications.root
      .locator('[data-testid^="row-"]')
      .filter({ hasText: title })
      .first();
    await expect(row).toBeVisible({ timeout: 45_000 });
    // A freshly uploaded file is version 1, and cannot be downloaded until its
    // virus scan reports clean — so the download action is not offered yet.
    await expect(row).toContainText('1');

    // Sharing mints a public link whose token is shown exactly once — the
    // warning appears with the URL, after the link is created, not before.
    await row.getByRole('button', { name: t('documentManagement.share') }).click();
    const share = new FormDialog(page, t);
    await share.waitOpen();
    await share.field('purpose').fill('e2e share');
    await share
      .field('expiresAt')
      .fill(new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString().slice(0, 16));
    await share.save();
    await expect(share.root.getByText(t('documentManagement.shareTokenWarning'))).toBeVisible({
      timeout: 45_000,
    });
    await expect(share.field('shareUrl')).not.toHaveValue('');
    await share.cancel();

    // Removing the reference takes the document off the record.
    await row.getByRole('button', { name: t('documentManagement.delete') }).click();
    await new ConfirmDialog(page, t).confirm();
    await expect(
      qualifications.root.locator('[data-testid^="row-"]').filter({ hasText: title })
    ).toHaveCount(0, { timeout: 45_000 });

    fs.rmSync(file, { force: true });
  });

  test('the upload dialog offers the document types the account configured', async ({
    page,
    shell,
    t,
    api,
    cleanup,
  }) => {
    const staffName = uniqueName('staff');
    const staffId = await api.createStaffMember(staffName);
    cleanup.add(`staff ${staffName}`, () => api.deactivateStaffMember(staffId));

    const category = `e2e-type-${unique()}`;
    const documentTypeId = await api.ensureDocumentType(category);
    cleanup.add(`document type ${category}`, () => api.disableDocumentType(documentTypeId));

    await shell.open('manageAdmin');
    const qualifications = new Section(page, 'staff-qualifications', t);
    await qualifications.expand();
    await pickOption(page, qualifications.root.locator('#qualificationStaffId'), staffName, true);

    await qualifications.root
      .getByRole('button', { name: t('documentManagement.upload') })
      .click();
    const form = new FormDialog(page, t);
    await form.waitOpen();

    // A configured type should be offered as a CHOICE, not typed by hand.
    await expect(form.root.getByRole('combobox', { name: t('documentManagement.category') })).toBeVisible();
    await form.select('category', category);
    await form.cancel();
  });

  test('a manager mints a public link and revokes it', async ({
    page,
    shell,
    t,
    api,
    cleanup,
  }) => {
    const unitName = uniqueName('unit');
    const transporterId = await api.createTransporter(unitName);
    cleanup.add(`unit ${unitName}`, () => api.deleteTransporter(transporterId));

    const links = new CrudFlow(page, 'public-links', t);
    await shell.open('manageAdmin');
    await links.open();

    await links.section.clickAdd();
    await links.form.waitOpen();
    await links.form.save();
    await expect(links.form.root).toBeVisible();
    await expect(links.form.anyError.first()).toBeVisible();

    const expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString().slice(0, 16);
    await links.form.fill({
      resourceType: 'Transporter',
      resourceId: transporterId,
      scopes: 'track',
      purpose: 'e2e',
      expiresAt,
    });
    await links.form.save();

    // The dialog stays open on purpose after a successful mint: the token is
    // shown once and never again.
    await expect(links.form.field('mintedToken')).not.toHaveValue('', { timeout: 45_000 });
    await links.form.cancel();

    const row = await links.section.findRow(transporterId);
    await expect(row).toContainText('track');

    await links.reload(shell, 'manageAdmin');
    const stored = await links.section.findRow(transporterId);
    await stored.getByRole('button', { name: t('publicLinks.revoke') }).click();
    await expect
      .poll(async () => (await links.section.findRow(transporterId)).innerText(), {
        timeout: 45_000,
      })
      .toContain(t('publicLinks.revokedAt'));
  });
});
