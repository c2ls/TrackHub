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
  extensions?: { code?: string };
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
  // TrackHub.TripManagement Domain/Constants/TripConstants.cs → TripErrorCodes
  TRIP_NOT_ACTIVE: 'errors.tripNotActive',
  TRIP_ALREADY_TERMINAL: 'errors.tripAlreadyTerminal',
  TRIP_INVALID_TRANSITION: 'errors.tripInvalidTransition',
  STOP_ALREADY_DEPARTED: 'errors.stopAlreadyDeparted',
  STOP_NOT_ARRIVED: 'errors.stopNotArrived',
  STOP_ALREADY_SKIPPED: 'errors.stopAlreadySkipped',
  TRIP_STOPS_NOT_COMPLETE: 'errors.tripStopsNotComplete',
  POD_DOCUMENT_NOT_CLEAN: 'errors.podDocumentNotClean',
  TRIP_DUPLICATE_CODE: 'errors.tripDuplicateCode',
  TRIP_DUPLICATE_EXTERNAL_REFERENCE: 'errors.tripDuplicateExternalReference',
  TRIP_HAS_HISTORY: 'errors.tripHasHistory',
  TRIP_DRIVER_NOT_ASSIGNABLE: 'errors.tripDriverNotAssignable',
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
    return new ApiError(errors.map((e) => e.message ?? 'Unknown GraphQL error').join('\n'), {
      code: errors.length > 0 ? codeOf(errors[0]) : undefined,
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

/** Dispatches an error to the global notification toast (NotificationContext). */
export function notifyApiError(error: unknown): void {
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
