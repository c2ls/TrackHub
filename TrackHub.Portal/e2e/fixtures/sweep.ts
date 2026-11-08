/**
 * Safety net for rows a crashed run left behind.
 *
 * Per-test cleanup is the primary mechanism (see `CleanupRegistry`); this sweep
 * is what keeps a killed run from poisoning the next one. It only ever touches
 * rows whose visible name carries the `e2e-<runId>-<n>` marker, and by default
 * only those older than an hour — a run in flight is never disturbed.
 */

import { ApiClient } from './api';
import { isStale } from './data';

export interface SweepReport {
  deleted: string[];
  failed: string[];
}

type Named = Record<string, unknown>;

const nameOf = (row: Named, ...fields: string[]): string =>
  fields.map((field) => String(row[field] ?? '')).join(' ');

export async function sweep(
  api: ApiClient,
  options: { olderThanMs?: number; includeCurrentRun?: boolean } = {}
): Promise<SweepReport> {
  const report: SweepReport = { deleted: [], failed: [] };
  const stale = (text: string): boolean =>
    options.includeCurrentRun ? /e2e-[0-9a-z]+-\d+/.test(text) : isStale(text, options.olderThanMs);

  const drop = async (
    label: string,
    backend: 'manager' | 'security' | 'geofencing' | 'tripManagement',
    mutation: string,
    variables: Record<string, unknown>
  ): Promise<void> => {
    const result = await api.tryGql(backend, mutation, variables);
    (result ? report.deleted : report.failed).push(label);
  };

  const accountId = await api.accountId().catch(() => null);

  /* Trips first: a `Created` trip holds its unit's arming slot, and a trip that
     acquired history can be retired only by CANCEL — abort is refused from
     `Created`, and delete is refused once events exist (spec 11b). */
  const trips = await api.tryGql<{ trips: { items: Named[] } }>(
    'tripManagement',
    'query($take: Int) { trips(query: { take: $take }) { items { tripId code status } } }',
    { take: 500 }
  );
  for (const trip of trips?.trips.items ?? []) {
    const code = String(trip.code ?? '');
    if (!stale(code)) continue;
    const id = trip.tripId as string;
    let released = await api.tryGql('tripManagement', 'mutation($id: UUID!) { deleteTrip(id: $id) }', { id });
    if (!released) {
      released = await api.tryGql(
        'tripManagement',
        'mutation($tripId: UUID!, $reason: String!) { abortTrip(command: { tripId: $tripId, reason: $reason }) }',
        { tripId: id, reason: 'e2e sweep' }
      );
      if (!released) {
        released = await api.tryGql(
          'tripManagement',
          'mutation($tripId: UUID!, $reason: String!) { cancelTrip(command: { tripId: $tripId, reason: $reason }) }',
          { tripId: id, reason: 'e2e sweep' }
        );
      }
      // Abort/cancel retire a trip but leave the row; delete is what removes it.
      if (released) {
        await api.tryGql('tripManagement', 'mutation($id: UUID!) { deleteTrip(id: $id) }', { id });
      }
    }
    // A trip that acquired history can be RETIRED but never removed: the backend
    // answers TRIP_HAS_HISTORY to every delete. Cancelled is the terminal state
    // that matters — it holds no arming slot — so a surviving row is not a leak.
    (released ? report.deleted : report.failed).push(`trip ${code}`);
  }

  const geofences = await api.tryGql<{ geofencesByAccount: { items: Named[] } }>(
    'geofencing',
    'query($take: Int) { geofencesByAccount(query: { take: $take }) { items { geofenceId name } } }',
    { take: 500 }
  );
  for (const row of geofences?.geofencesByAccount.items ?? []) {
    if (!stale(nameOf(row, 'name'))) continue;
    await drop(`geofence ${row.name}`, 'geofencing', 'mutation($id: UUID!) { deleteGeofence(id: $id) }', {
      id: row.geofenceId,
    });
  }

  const transporters = await api.tryGql<{ transportersByAccount: { items: Named[] } }>(
    'manager',
    'query($take: Int) { transportersByAccount(query: { take: $take }) { items { transporterId name } } }',
    { take: 500 }
  );
  for (const row of transporters?.transportersByAccount.items ?? []) {
    if (!stale(nameOf(row, 'name'))) continue;
    await drop(`unit ${row.name}`, 'manager', 'mutation($id: UUID!) { deleteTransporter(id: $id) }', {
      id: row.transporterId,
    });
  }

  const groups = await api.tryGql<{ groupsByAccount: { items: Named[] } }>(
    'manager',
    'query($take: Int) { groupsByAccount(query: { take: $take }) { items { groupId name } } }',
    { take: 500 }
  );
  for (const row of groups?.groupsByAccount.items ?? []) {
    if (!stale(nameOf(row, 'name'))) continue;
    await drop(`group ${row.name}`, 'manager', 'mutation($id: Long!) { deleteGroup(id: $id) }', {
      id: row.groupId,
    });
  }

  const pois = await api.tryGql<{ pointsOfInterestByAccount: { items: Named[] } }>(
    'manager',
    'query($take: Int) { pointsOfInterestByAccount(query: { take: $take }) { items { pointOfInterestId name } } }',
    { take: 500 }
  );
  for (const row of pois?.pointsOfInterestByAccount.items ?? []) {
    if (!stale(nameOf(row, 'name'))) continue;
    await drop(`poi ${row.name}`, 'manager', 'mutation($id: UUID!) { deletePointOfInterest(id: $id) }', {
      id: row.pointOfInterestId,
    });
  }

  if (accountId) {
    const staff = await api.tryGql<{ staffMembersByAccount: { items: Named[] } }>(
      'manager',
      'query($accountId: UUID!, $skip: Int!, $take: Int!) { staffMembersByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) { items { staffId name active } } }',
      { accountId, skip: 0, take: 500 }
    );
    for (const row of staff?.staffMembersByAccount.items ?? []) {
      if (!stale(nameOf(row, 'name')) || row.active === false) continue;
      await drop(
        `staff ${row.name}`,
        'manager',
        'mutation($staffId: UUID!) { deactivateStaffMember(command: { staffId: $staffId }) }',
        { staffId: row.staffId }
      );
    }

    const rules = await api.tryGql<{ notificationRules: { items: Named[] } }>(
      'manager',
      'query($accountId: UUID!, $skip: Int!, $take: Int!) { notificationRules(query: { accountId: $accountId, skip: $skip, take: $take }) { items { notificationRuleId ruleKey enabled } } }',
      { accountId, skip: 0, take: 500 }
    );
    for (const row of rules?.notificationRules.items ?? []) {
      if (!stale(nameOf(row, 'ruleKey')) || row.enabled === false) continue;
      await drop(
        `notification rule ${row.ruleKey}`,
        'manager',
        'mutation($notificationRuleId: UUID!) { disableNotificationRule(command: { notificationRuleId: $notificationRuleId }) }',
        { notificationRuleId: row.notificationRuleId }
      );
    }

    const templates = await api.tryGql<{ notificationTemplates: Named[] }>(
      'manager',
      'query($accountId: UUID!) { notificationTemplates(query: { accountId: $accountId }) { notificationTemplateId templateKey } }',
      { accountId }
    );
    for (const row of templates?.notificationTemplates ?? []) {
      if (!stale(nameOf(row, 'templateKey'))) continue;
      await drop(
        `notification template ${row.templateKey}`,
        'manager',
        'mutation($notificationTemplateId: UUID!) { deleteNotificationTemplate(command: { notificationTemplateId: $notificationTemplateId }) }',
        { notificationTemplateId: row.notificationTemplateId }
      );
    }

    const documentTypes = await api.tryGql<{ documentTypes: Named[] }>(
      'manager',
      'query($accountId: UUID!) { documentTypes(query: { accountId: $accountId, includeDisabled: false }) { documentTypeId displayName } }',
      { accountId }
    );
    for (const row of documentTypes?.documentTypes ?? []) {
      if (!stale(nameOf(row, 'displayName'))) continue;
      await drop(
        `document type ${row.displayName}`,
        'manager',
        'mutation($documentTypeId: UUID!) { disableDocumentType(command: { documentTypeId: $documentTypeId }) }',
        { documentTypeId: row.documentTypeId }
      );
    }
  }

  const announcements = await api.tryGql<{ platformAnnouncements: Named[] }>(
    'manager',
    'query($skip: Int!, $take: Int!) { platformAnnouncements(query: { skip: $skip, take: $take }) { platformAnnouncementId messageEn } }',
    { skip: 0, take: 200 }
  );
  for (const row of announcements?.platformAnnouncements ?? []) {
    if (!stale(nameOf(row, 'messageEn'))) continue;
    await drop(
      `announcement ${row.messageEn}`,
      'manager',
      'mutation($platformAnnouncementId: UUID!) { deletePlatformAnnouncement(command: { platformAnnouncementId: $platformAnnouncementId }) }',
      { platformAnnouncementId: row.platformAnnouncementId }
    );
  }

  // Public link grants carry no name, but the suite always stamps its purpose.
  if (accountId) {
    const links = await api.tryGql<{ publicLinkGrantsByAccount: { items: Named[] } }>(
      'manager',
      'query($accountId: UUID!, $skip: Int!, $take: Int!) { publicLinkGrantsByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) { items { publicLinkGrantId purpose revokedAt } } }',
      { accountId, skip: 0, take: 500 }
    );
    for (const row of links?.publicLinkGrantsByAccount.items ?? []) {
      if (row.revokedAt) continue;
      if (!String(row.purpose ?? '').toLowerCase().includes('e2e')) continue;
      await drop(
        `public link ${row.publicLinkGrantId}`,
        'manager',
        'mutation($publicLinkGrantId: UUID!) { revokePublicLinkGrant(command: { publicLinkGrantId: $publicLinkGrantId }) }',
        { publicLinkGrantId: row.publicLinkGrantId }
      );
    }
  }

  const providers = await api.tryGql<{ geocodingProviders: Named[] }>(
    'manager',
    'query { geocodingProviders { geocodingProviderId name } }'
  );
  for (const row of providers?.geocodingProviders ?? []) {
    if (!stale(nameOf(row, 'name'))) continue;
    await drop(
      `geocoding provider ${row.name}`,
      'manager',
      'mutation($id: UUID!) { deleteGeocodingProvider(id: $id) }',
      { id: row.geocodingProviderId }
    );
  }

  const tollClasses = await api.tryGql<{ tollVehicleClasses: Named[] }>(
    'tripManagement',
    'query { tollVehicleClasses { tollVehicleClassId code name active } }'
  );
  for (const row of tollClasses?.tollVehicleClasses ?? []) {
    if (!stale(nameOf(row, 'code', 'name')) || row.active === false) continue;
    await drop(
      `toll class ${row.code}`,
      'tripManagement',
      'mutation($id: UUID!) { deactivateTollVehicleClass(id: $id) }',
      { id: row.tollVehicleClassId }
    );
  }

  // Toll stations are PLATFORM reference data (SVD-12), so a leaked one is
  // worse than a leaked account row: it shows up in every account's catalog and
  // is priced into every route estimate. Swept by name like everything else.
  const tollStations = await api.tryGql<{ tollStations: { items: Named[] } }>(
    'tripManagement',
    'query { tollStations(query: { take: 500 }) { items { tollStationId name active } } }'
  );
  for (const row of tollStations?.tollStations.items ?? []) {
    if (!stale(nameOf(row, 'name')) || row.active === false) continue;
    await drop(
      `toll station ${row.name}`,
      'tripManagement',
      'mutation($id: UUID!) { deactivateTollStation(id: $id) }',
      { id: row.tollStationId }
    );
  }

  // These rows carry no run marker — the resource name is the only handle — and
  // only this suite ever creates one with that prefix.
  const servicePermissions = await api.tryGql<{ serviceClientPermissions: Named[] }>(
    'security',
    'query($skip: Int!, $take: Int!) { serviceClientPermissions(query: { skip: $skip, take: $take }) { serviceClientPermissionId resource } }',
    { skip: 0, take: 500 }
  );
  for (const row of servicePermissions?.serviceClientPermissions ?? []) {
    // These carry no run marker — the resource name is the only handle — so the
    // sweep matches the prefix the suite always uses.
    if (!String(row.resource ?? '').startsWith('E2eResource')) continue;
    await drop(
      `service client permission ${row.resource}`,
      'security',
      'mutation($serviceClientPermissionId: UUID!) { deleteServiceClientPermission(command: { serviceClientPermissionId: $serviceClientPermissionId }) }',
      { serviceClientPermissionId: row.serviceClientPermissionId }
    );
  }

  const clients = await api.tryGql<{ clients: Named[] }>(
    'security',
    'query($skip: Int!, $take: Int!) { clients(query: { skip: $skip, take: $take }) { clientId name } }',
    { skip: 0, take: 200 }
  );
  for (const row of clients?.clients ?? []) {
    if (!stale(nameOf(row, 'name'))) continue;
    await drop(`client ${row.name}`, 'security', 'mutation($id: UUID!) { deleteClient(id: $id) }', {
      id: row.clientId,
    });
  }

  // Users last: deleting the one this run signed in as would break every step above.
  const users = await api.tryGql<{ usersByAccount: { items: Named[] } }>(
    'security',
    'query($take: Int) { usersByAccount(query: { take: $take }) { items { userId username emailAddress } } }',
    { take: 500 }
  );
  for (const row of users?.usersByAccount.items ?? []) {
    if (!stale(nameOf(row, 'username', 'emailAddress'))) continue;
    await drop(`user ${row.username}`, 'security', 'mutation($id: UUID!) { deleteUser(id: $id) }', {
      id: row.userId,
    });
  }

  return report;
}
