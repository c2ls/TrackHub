/**
 * Thin backend client for SEEDING and CLEANUP only.
 *
 * Rules it lives by:
 *  - it never replaces a UI assertion. Everything a user can do is exercised
 *    through the browser; this client exists so a crashed run cannot leave
 *    `e2e-*` rows behind, and so a test can arrange a precondition the UI has no
 *    screen for.
 *  - it talks to the same endpoints the portal is configured with (fixtures/env)
 *    and authenticates with a token captured from a REAL browser sign-in — the
 *    OAuth flow is never reimplemented here.
 */

import fs from 'node:fs';
import { request } from '@playwright/test';
import type { APIRequestContext } from '@playwright/test';
import { config, tokenPath } from './env';
import type { RoleName } from './env';

/** What the setup project captures from the browser's token exchange. */
export interface CapturedTokens {
  access_token: string;
  refresh_token?: string;
  /** Epoch ms when the access token was issued. */
  obtained_at: number;
  expires_in?: number;
}

interface GraphQLError {
  message: string;
  extensions?: { code?: string };
}

export class ApiError extends Error {
  constructor(message: string, readonly code?: string) {
    super(message);
    this.name = 'ApiError';
  }
}

type Backend = keyof typeof config.graphql;

export class ApiClient {
  private context: APIRequestContext | null = null;
  private tokens: CapturedTokens | null = null;

  constructor(private readonly role: RoleName = 'admin') {}

  private async http(): Promise<APIRequestContext> {
    this.context ??= await request.newContext({ ignoreHTTPSErrors: true });
    return this.context;
  }

  async dispose(): Promise<void> {
    await this.context?.dispose();
    this.context = null;
  }

  /** True when a browser sign-in for this role has been recorded. */
  get available(): boolean {
    return fs.existsSync(tokenPath(this.role));
  }

  private load(): CapturedTokens {
    if (this.tokens) return this.tokens;
    const file = tokenPath(this.role);
    if (!fs.existsSync(file)) {
      throw new ApiError(`No captured token for role "${this.role}" — run the setup project first.`);
    }
    this.tokens = JSON.parse(fs.readFileSync(file, 'utf8')) as CapturedTokens;
    return this.tokens;
  }

  /**
   * A valid access token. Refreshes through the OAuth refresh grant when the
   * captured one is close to expiry; OpenIddict rotates refresh tokens, so the
   * new pair is written back or the next refresh would present a spent token.
   */
  async accessToken(): Promise<string> {
    const tokens = this.load();
    const lifetime = (tokens.expires_in ?? 3600) * 1000;
    const ageLeft = tokens.obtained_at + lifetime - Date.now();
    if (ageLeft > 60_000) return tokens.access_token;
    if (!tokens.refresh_token) return tokens.access_token;

    const http = await this.http();
    const response = await http.post(config.tokenEndpoint!, {
      form: {
        grant_type: 'refresh_token',
        refresh_token: tokens.refresh_token,
        client_id: config.clientId,
      },
    });
    if (!response.ok()) {
      // Nothing to recover from here: the caller's next request will fail with a
      // 401 that says the same thing, and hiding it behind a retry loop would
      // turn an expired suite token into a mystery.
      throw new ApiError(`Refreshing the ${this.role} token failed: ${response.status()}`);
    }
    const body = (await response.json()) as CapturedTokens;
    this.tokens = { ...body, obtained_at: Date.now() };
    fs.writeFileSync(tokenPath(this.role), JSON.stringify(this.tokens, null, 2));
    return this.tokens.access_token;
  }

  /** Executes a GraphQL document against one of the portal's configured backends. */
  async gql<T = Record<string, unknown>>(
    backend: Backend,
    query: string,
    variables: Record<string, unknown> = {}
  ): Promise<T> {
    const url = config.graphql[backend];
    if (!url) throw new ApiError(`Backend "${backend}" is not configured in the portal env.`);
    const http = await this.http();
    const response = await http.post(url, {
      headers: {
        Authorization: `Bearer ${await this.accessToken()}`,
        'Content-Type': 'application/json',
      },
      data: { query, variables },
    });
    const payload = (await response.json()) as { data?: T; errors?: GraphQLError[] };
    if (payload.errors?.length) {
      const first = payload.errors[0];
      throw new ApiError(`${backend}: ${first.message}`, first.extensions?.code);
    }
    return payload.data as T;
  }

  /** Same as {@link gql}, but a failure resolves to `null` — for best-effort cleanup. */
  async tryGql<T = Record<string, unknown>>(
    backend: Backend,
    query: string,
    variables: Record<string, unknown> = {}
  ): Promise<T | null> {
    try {
      return await this.gql<T>(backend, query, variables);
    } catch {
      return null;
    }
  }

  /** Authenticated REST call (documents, reports). Returns the raw response. */
  async rest(method: 'get' | 'post' | 'delete', url: string, data?: unknown) {
    const http = await this.http();
    return http.fetch(url, {
      method,
      headers: { Authorization: `Bearer ${await this.accessToken()}` },
      data: data as never,
    });
  }

  /** Unauthenticated REST call — the public tracking and announcement surfaces. */
  async anonymous(url: string) {
    const http = await this.http();
    return http.get(url);
  }

  /* ------------------------------------------------------------ lookups */

  async accountId(): Promise<string> {
    const data = await this.gql<{ accountByUser: { accountId: string } }>(
      'manager',
      'query { accountByUser { accountId name } }'
    );
    return data.accountByUser.accountId;
  }

  /**
   * Creates a unit for a test that needs one.
   *
   * Deliberately not through the UI: Account Management renders no "Add" for
   * units (reported as a finding), so the browser has no way to make one.
   */
  async createTransporter(name: string, transporterTypeId = 1): Promise<string> {
    const data = await this.gql<{ createTransporter: { transporterId: string } }>(
      'manager',
      'mutation($transporter: TransporterDtoInput!) { createTransporter(command: { transporter: $transporter }) { transporterId name } }',
      { transporter: { name, transporterTypeId, accountId: await this.accountId() } }
    );
    return data.createTransporter.transporterId;
  }

  /**
   * Registers a staff for a test that needs one.
   *
   * Seeded rather than clicked because the staff record is the PRECONDITION
   * here — what the test measures is the credential, qualification and
   * assignment work that hangs off it, and creating the staff through the UI
   * is already covered by its own test.
   */
  async createStaffMember(name: string, defaultTransporterId?: string): Promise<string> {
    const data = await this.gql<{ createStaffMember: { staffId: string } }>(
      'manager',
      'mutation($staff: StaffMemberDtoInput!) { createStaffMember(command: { staff: $staff }) { staffId name } }',
      {
        staff: {
          name,
          active: true,
          staffType: 'Driver',
          accountId: await this.accountId(),
          defaultTransporterId: defaultTransporterId ?? null,
        },
      }
    );
    return data.createStaffMember.staffId;
  }

  /** Staff are deactivated, never deleted — that is the record's whole point. */
  async deactivateStaffMember(staffId: string): Promise<void> {
    await this.tryGql(
      'manager',
      'mutation($staffId: UUID!) { deactivateStaffMember(command: { staffId: $staffId }) }',
      { staffId }
    );
  }

  async deleteTransporter(transporterId: string): Promise<void> {
    await this.tryGql('manager', 'mutation($id: UUID!) { deleteTransporter(id: $id) }', {
      id: transporterId,
    });
  }

  /**
   * Creates a polygon geofence.
   *
   * Polygons are drawn with map clicks in `leaflet-editable`, which a browser
   * test can start but not finish reliably; the shape is seeded here so the
   * screen's edit/toggle/delete/filter behaviour is what the test measures.
   */
  async createPolygonGeofence(name: string, centre = { lat: 4.62, lng: -74.06 }): Promise<string> {
    const size = 0.01;
    const corners = [
      { latitude: centre.lat - size, longitude: centre.lng - size },
      { latitude: centre.lat - size, longitude: centre.lng + size },
      { latitude: centre.lat + size, longitude: centre.lng + size },
      { latitude: centre.lat + size, longitude: centre.lng - size },
      { latitude: centre.lat - size, longitude: centre.lng - size },
    ];
    const geofenceId = crypto.randomUUID();
    await this.gql(
      'geofencing',
      'mutation($geofence: GeofenceDtoInput!) { createGeofence(command: { geofence: $geofence }) { geofenceId name } }',
      {
        geofence: {
          geofenceId,
          name,
          description: 'seeded by the e2e suite',
          color: 2,
          type: 1,
          active: true,
          alertOnEntry: false,
          alertOnExit: false,
          geom: { srid: 4326, coordinates: corners },
        },
      }
    );
    return geofenceId;
  }

  async deleteGeofence(geofenceId: string): Promise<void> {
    await this.tryGql('geofencing', 'mutation($id: UUID!) { deleteGeofence(id: $id) }', {
      id: geofenceId,
    });
  }

  /**
   * Retires a trip, whatever state it is in.
   *
   * A `Created` trip holds its unit's arming slot (spec 11b), and the three
   * verbs are not interchangeable: delete is refused once the trip has history,
   * abort is refused from `Created`, and cancel is the only one that retires a
   * queued trip that already acquired events. Trying all three in order is what
   * makes the release actually succeed.
   */
  async releaseTrip(tripId: string): Promise<void> {
    // A customer tracking link outlives the trip it points at, so revoke first:
    // once the trip is gone the grant is still an unrevoked row.
    const detail = await this.tryGql<{ tripDetail: { shares: { tripShareId: string; revokedAt: string | null }[] } }>(
      'tripManagement',
      'query($tripId: UUID!) { tripDetail(query: { tripId: $tripId }) { shares { tripShareId revokedAt } } }',
      { tripId }
    );
    for (const share of detail?.tripDetail.shares ?? []) {
      if (share.revokedAt) continue;
      await this.tryGql(
        'tripManagement',
        'mutation($tripId: UUID!, $tripShareId: UUID!) { revokeTripShare(command: { tripId: $tripId, tripShareId: $tripShareId }) }',
        { tripId, tripShareId: share.tripShareId }
      );
    }

    const deleted = await this.tryGql(
      'tripManagement',
      'mutation($id: UUID!) { deleteTrip(id: $id) }',
      { id: tripId }
    );
    if (deleted) return;

    const aborted = await this.tryGql(
      'tripManagement',
      'mutation($tripId: UUID!, $reason: String!) { abortTrip(command: { tripId: $tripId, reason: $reason }) }',
      { tripId, reason: 'e2e cleanup' }
    );
    if (!aborted) {
      await this.tryGql(
        'tripManagement',
        'mutation($tripId: UUID!, $reason: String!) { cancelTrip(command: { tripId: $tripId, reason: $reason }) }',
        { tripId, reason: 'e2e cleanup' }
      );
    }
    await this.tryGql('tripManagement', 'mutation($id: UUID!) { deleteTrip(id: $id) }', {
      id: tripId,
    });
  }

  /** The account's units, for a test that needs one it did not create. */
  /**
   * Ensures the account has an enabled document type.
   *
   * The upload dialog's category picker is fed by the account's document types,
   * and an account with none (or whose only types were disabled) can store no
   * document at all — so this is a precondition, not the thing under test.
   */
  async ensureDocumentType(category: string): Promise<string> {
    const data = await this.gql<{ configureDocumentType: { documentTypeId: string } }>(
      'manager',
      'mutation($documentType: DocumentTypeDtoInput!) { configureDocumentType(command: { documentType: $documentType }) { documentTypeId category } }',
      {
        documentType: {
          accountId: await this.accountId(),
          category,
          displayName: category,
          required: false,
          expiring: false,
        },
      }
    );
    return data.configureDocumentType.documentTypeId;
  }

  async disableDocumentType(documentTypeId: string): Promise<void> {
    await this.tryGql(
      'manager',
      'mutation($documentTypeId: UUID!) { disableDocumentType(command: { documentTypeId: $documentTypeId }) }',
      { documentTypeId }
    );
  }

  /**
   * The units the SIGNED-IN user can see, which is what every picker on the
   * trip screen is fed from. Not the same as the account's units: visibility is
   * group-scoped, so a unit seeded without group membership is invisible here.
   */
  async visibleTransporters(): Promise<{ transporterId: string; name: string }[]> {
    const data = await this.gql<{
      transporterLookupByUser: { transporterId: string; name: string }[];
    }>('manager', 'query { transporterLookupByUser { transporterId name } }');
    return data.transporterLookupByUser;
  }

  async transporterLookup(): Promise<{ transporterId: string; name: string }[]> {
    const data = await this.gql<{
      transporterLookupByAccount: { transporterId: string; name: string }[];
    }>('manager', 'query { transporterLookupByAccount { transporterId name } }');
    return data.transporterLookupByAccount;
  }

  async currentUserId(): Promise<string> {
    const data = await this.gql<{ currentPrincipal: { userId: string } }>(
      'manager',
      'query { currentPrincipal { userId principalType } }'
    );
    return data.currentPrincipal.userId;
  }
}

export const adminApi = new ApiClient('admin');
