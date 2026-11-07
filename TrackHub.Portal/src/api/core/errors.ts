/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
*
*  Licensed under the Apache License, Version 2.0 (the "License").
*  You may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
*/

import type { AppErrorDetail } from 'utils/errorHandler';

export interface GraphQLErrorEntry {
  message?: string;
  extensions?: { code?: string; errors?: Record<string, string[]> };
  code?: string;
}

/**
 * Maps well-known backend error codes to user-facing i18n keys. Covers the
 * Reporting REST envelope codes, the shared account/feature codes, and the
 * TripManagement rejection codes (`TripErrorCodes`), so both blob/JSON REST
 * responses and GraphQL error extensions surface as friendly localized toasts
 * rather than raw server text.
 *
 * Codes with no entry fall through to the server's message — deliberately: an
 * unmapped code is better shown verbatim than hidden behind a generic string.
 */
export const ERROR_CODE_I18N: Record<string, string> = {
  FEATURE_DISABLED: 'errors.featureDisabled',
  ACCOUNT_SUSPENDED: 'errors.accountSuspended',
  REPORT_ACCESS_DENIED: 'errors.reportAccessDenied',
  REPORT_NOT_FOUND: 'errors.reportNotFound',
  UNSUPPORTED_REPORT_FORMAT: 'errors.unsupportedReportFormat',
  REPORT_ROW_LIMIT_EXCEEDED: 'errors.reportRowLimitExceeded',
  // Raised by the unpaged picker lookups past their 5000-row ceiling, and by the
  // seeded permission catalogs past theirs. Both refuse to truncate, so the
  // picker must say why it is empty rather than silently rendering nothing.
  LOOKUP_LIMIT_EXCEEDED: 'errors.lookupLimitExceeded',
  SEEDED_CATALOG_LIMIT_EXCEEDED: 'errors.seededCatalogLimitExceeded',
  UNPAGED_READ_LIMIT_EXCEEDED: 'errors.unpagedReadLimitExceeded',
  FORBIDDEN: 'errors.forbidden',
  UNAUTHORIZED: 'errors.unauthorized',
  TOO_MANY_REQUESTS: 'errors.tooManyRequests',
  UPSTREAM_ERROR: 'errors.upstreamError',
  CONCURRENT_UPDATE: 'errors.concurrentUpdate',
  INVALID_CURSOR: 'errors.invalidCursor',
  PUBLIC_LINK_EXPIRY_TOO_LONG: 'errors.publicLinkExpiryTooLong',
  GEOFENCE_ID_MISMATCH: 'errors.geofenceIdMismatch',
  REPORT_WINDOW_INVALID: 'errors.reportWindowInvalid',
  OPERATOR_NOT_FOUND: 'errors.operatorNotFound',
  OPERATOR_DISABLED: 'errors.operatorDisabled',
  PROVIDER_CAPABILITY_NOT_SUPPORTED: 'errors.providerCapabilityNotSupported',
  GEOCODER_UNAVAILABLE: 'errors.geocoderUnavailable',
  POSITION_HISTORY_LIMIT_EXCEEDED: 'errors.positionHistoryLimitExceeded',
  // TrackHub.TripManagement Domain/Constants/TripConstants.cs → TripErrorCodes
  TRIP_NOT_FOUND: 'errors.tripNotFound',
  TRIP_FORBIDDEN: 'errors.tripForbidden',
  TRIP_NOT_ACTIVE: 'errors.tripNotActive',
  TRIP_ALREADY_TERMINAL: 'errors.tripAlreadyTerminal',
  TRIP_MODIFIED_CONCURRENTLY: 'errors.tripModifiedConcurrently',
  TRIP_INVALID_TRANSITION: 'errors.tripInvalidTransition',
  STOP_ALREADY_DEPARTED: 'errors.stopAlreadyDeparted',
  STOP_NOT_ARRIVED: 'errors.stopNotArrived',
  STOP_ALREADY_SKIPPED: 'errors.stopAlreadySkipped',
  TRIP_STOPS_NOT_COMPLETE: 'errors.tripStopsNotComplete',
  POD_DOCUMENT_NOT_CLEAN: 'errors.podDocumentNotClean',
  POD_DOCUMENT_INVALID: 'errors.podDocumentInvalid',
  POD_REQUIRED: 'errors.podRequired',
  DELIVERY_OUTCOME_RECORDED: 'errors.deliveryOutcomeRecorded',
  TRIP_EVENT_TIME_OUT_OF_RANGE: 'errors.tripEventTimeOutOfRange',
  TRIP_IMPORT_INVALID_ROW: 'errors.tripImportInvalidRow',
  TOLL_IMPORT_INVALID_ROW: 'errors.tollImportInvalidRow',
  TRIP_DUPLICATE_CODE: 'errors.tripDuplicateCode',
  TRIP_DUPLICATE_EXTERNAL_REFERENCE: 'errors.tripDuplicateExternalReference',
  TRIP_HAS_HISTORY: 'errors.tripHasHistory',
  TRIP_STAFF_NOT_ASSIGNABLE: 'errors.tripStaffNotAssignable',
  TRIP_TRANSPORTER_BUSY: 'errors.tripTransporterBusy',
  TRIP_ARMED: 'errors.tripArmed',
  TRIP_START_EVIDENCE_REQUIRED: 'errors.tripStartEvidenceRequired',
  ROUTING_NOT_CONFIGURED: 'errors.routingNotConfigured',
  ROUTING_UNAVAILABLE: 'errors.routingUnavailable',
  ROUTING_INVALID_GEOMETRY: 'errors.routingInvalidGeometry',
  TOLL_OVERLAPPING_TARIFF: 'errors.tollOverlappingTariff',
  TOLL_DUPLICATE_STATION: 'errors.tollDuplicateStation',
  TOLL_DUPLICATE_VEHICLE_CLASS: 'errors.tollDuplicateVehicleClass',
  UNKNOWN_VEHICLE_CLASS: 'errors.unknownVehicleClass',
  TRIP_SHARE_REVOKED: 'errors.tripShareRevoked',
};

/**
 * Extracts the `{ errors: [...] }` envelope from a REST error response body.
 * The download path receives the body as a `Blob` (responseType: 'blob'); the
 * preview/JSON path receives a parsed object. Returns `[]` when nothing parses.
 */
export async function extractRestErrorEntries(data: unknown): Promise<GraphQLErrorEntry[]> {
  if (data == null) return [];
  const parse = (text: string): GraphQLErrorEntry[] => {
    try {
      const parsed = JSON.parse(text) as { errors?: GraphQLErrorEntry[] };
      return parsed.errors ?? [];
    } catch {
      return [];
    }
  };
  if (typeof Blob !== 'undefined' && data instanceof Blob) {
    return parse(await data.text());
  }
  if (typeof data === 'string') return parse(data);
  if (typeof data === 'object') {
    return (data as { errors?: GraphQLErrorEntry[] }).errors ?? [];
  }
  return [];
}

// See fromGraphQLErrors.
const REFUSAL_CODES = new Set(['VALIDATION_ERROR', 'CONFLICT', 'NOT_FOUND']);

function refusalMessage(entry: GraphQLErrorEntry): string {
  const fieldMessages = Object.values(entry.extensions?.errors ?? {}).flat();
  return fieldMessages.length > 0 ? fieldMessages.join(' ') : (entry.message ?? 'The request could not be completed.');
}

/** Per-call transport options; React Query hands `signal` to every queryFn. */
export interface RequestOptions {
  signal?: AbortSignal;
}

export const REQUEST_CANCELLED_CODE = 'REQUEST_CANCELLED';

// Checked against the caller's own signal as well as the axios code: the caller abandoning the
// request is what makes it a cancellation, whatever shape the transport reports it in.
export function isCancellation(error: unknown, signal: { readonly aborted: boolean } | undefined): boolean {
  return signal?.aborted === true || (error as { code?: string } | null)?.code === 'ERR_CANCELED';
}

export function isRequestCancelled(error: unknown): boolean {
  return error instanceof ApiError && error.code === REQUEST_CANCELLED_CODE;
}

/** Shown for any failure the UI has no specific explanation for. */
export const UNEXPECTED_ERROR_I18N_KEY = 'errors.unexpected';

/** Keeps the server's own words reachable while developing, without showing them to the user. */
function logUnmappedError(detail: string): void {
  if (process.env.NODE_ENV !== 'production') {
    console.debug('[api] unmapped server error:', detail);
  }
}

/**
 * Normalized API failure. The api layer THROWS these; deciding what a failure
 * means (fallback value, toast, retry) belongs to the caller — usually the
 * query layer's global error handler.
 */
export class ApiError extends Error {
  /** Well-known backend code (e.g. ACCOUNT_SUSPENDED, FEATURE_DISABLED) if present. */
  readonly code?: string;
  /** Translation key for user-facing display, when the code maps to one. */
  readonly i18nKey?: string;
  /** HTTP status, when the failure was a transport error. */
  readonly status?: number;
  /** Raw GraphQL error entries, when the failure came from a GraphQL payload. */
  readonly graphQLErrors: GraphQLErrorEntry[];

  constructor(
    message: string,
    options: {
      code?: string;
      i18nKey?: string;
      status?: number;
      graphQLErrors?: GraphQLErrorEntry[];
      cause?: unknown;
    } = {}
  ) {
    super(message, { cause: options.cause });
    this.name = 'ApiError';
    this.code = options.code;
    this.i18nKey = options.i18nKey;
    this.status = options.status;
    this.graphQLErrors = options.graphQLErrors ?? [];
  }

  /**
   * Builds an ApiError from a REST error envelope (`{ errors: [...] }`), mapping
   * well-known Reporting codes to friendly i18n keys. Falls back to the transport
   * message when no envelope/code is present.
   */
  static fromRestErrors(
    errors: GraphQLErrorEntry[],
    fallbackMessage: string,
    status?: number
  ): ApiError {
    const codeOf = (e: GraphQLErrorEntry) => e.extensions?.code ?? e.code;
    const mapped = errors.find((e) => {
      const code = codeOf(e);
      return code !== undefined && code in ERROR_CODE_I18N;
    });
    const entry = mapped ?? errors[0];
    const code = entry ? codeOf(entry) : undefined;
    return new ApiError(entry?.message ?? fallbackMessage, {
      code,
      i18nKey: code ? ERROR_CODE_I18N[code] : undefined,
      status,
      graphQLErrors: errors,
    });
  }

  static cancelled(cause: unknown): ApiError {
    return new ApiError('The request was cancelled.', { code: REQUEST_CANCELLED_CODE, cause });
  }

  static fromGraphQLErrors(errors: GraphQLErrorEntry[]): ApiError {
    const codeOf = (e: GraphQLErrorEntry) => e.extensions?.code ?? e.code;
    if (errors.some((e) => codeOf(e) === 'ACCOUNT_SUSPENDED')) {
      return new ApiError('This account is not currently operational.', {
        code: 'ACCOUNT_SUSPENDED',
        i18nKey: 'errors.accountSuspended',
        graphQLErrors: errors,
      });
    }
    if (errors.some((e) => codeOf(e) === 'FEATURE_DISABLED')) {
      return new ApiError('This feature is not enabled for your account.', {
        code: 'FEATURE_DISABLED',
        i18nKey: 'errors.featureDisabled',
        graphQLErrors: errors,
      });
    }
    // Any other mapped code (the TripManagement rejections, the report codes)
    // wins over the first entry, so a localized explanation beats raw server text.
    const mapped = errors.find((e) => {
      const code = codeOf(e);
      return code !== undefined && code in ERROR_CODE_I18N;
    });
    if (mapped) {
      const code = codeOf(mapped) as string;
      return new ApiError(mapped.message ?? code, {
        code,
        i18nKey: ERROR_CODE_I18N[code],
        graphQLErrors: errors,
      });
    }

    // A coded refusal carries text the handler wrote for a person ("An assignment for this template
    // and scope already exists.", the validator's per-field sentences), so it is shown as-is instead
    // of the generic line that hides why a save was refused.
    const refused = errors.find((e) => REFUSAL_CODES.has(codeOf(e) ?? ''));
    if (refused) {
      return new ApiError(refusalMessage(refused), {
        code: codeOf(refused),
        graphQLErrors: errors,
      });
    }

    // No code the UI knows: the server text is unmapped and may name a table, a constraint or an
    // upstream service, so the user gets the generic line and the detail goes to the console.
    logUnmappedError(errors.map((e) => e.message ?? 'Unknown GraphQL error').join('\n'));
    return new ApiError('The request could not be completed.', {
      code: errors.length > 0 ? codeOf(errors[0]) : undefined,
      i18nKey: UNEXPECTED_ERROR_I18N_KEY,
      graphQLErrors: errors,
    });
  }

  /** Bridge to the legacy `'app-error'` CustomEvent consumed by NotificationContext. */
  toAppErrorDetail(): AppErrorDetail {
    const type =
      this.code === 'ACCOUNT_SUSPENDED'
        ? 'account-suspended'
        : this.code === 'FEATURE_DISABLED'
          ? 'feature-disabled'
          : this.graphQLErrors.length > 0
            ? 'graphql'
            : 'network';
    return { message: this.message, type, code: this.code, i18nKey: this.i18nKey };
  }
}

// A coded refusal or a 4xx answers the same way twice; only a lost or failed transport is worth retrying.
export function isTransportFailure(error: unknown): boolean {
  if (!(error instanceof ApiError)) return false;
  if (error.code || error.graphQLErrors.length > 0) return false;
  return error.status === undefined || error.status >= 500;
}

/** Dispatches an error to the global notification toast (NotificationContext). */
export function notifyApiError(error: unknown): void {
  if (isRequestCancelled(error)) return;
  const detail: AppErrorDetail =
    error instanceof ApiError
      ? error.toAppErrorDetail()
      : {
          message: error instanceof Error ? error.message : 'Unexpected error',
          type: 'unexpected',
          code: undefined,
          i18nKey: undefined,
        };
  window.dispatchEvent(new CustomEvent<AppErrorDetail>('app-error', { detail }));
}
